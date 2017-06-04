using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;


public class TextureDetails {
	public bool isCubeMap;
	public int memSizeKB;
	public Texture texture;
	public TextureFormat format;
	public int mipMapCount;
	public List<Object> FoundInMaterials=new List<Object>();
	public List<Object> FoundInRenderers=new List<Object>();
	public List<Object> FoundInAnimators = new List<Object>();
	public List<Object> FoundInScripts = new List<Object>();
	public List<Object> FoundInGraphics = new List<Object>();
	public TextureDetails() {

	}
};

public class MaterialDetails
{

	public Material material;

	public List<Renderer> FoundInRenderers=new List<Renderer>();
	public List<Graphic> FoundInGraphics=new List<Graphic>();

	public MaterialDetails()
	{

	}
};

public class MeshDetails
{

	public Mesh mesh;

	public List<MeshFilter> FoundInMeshFilters=new List<MeshFilter>();
	public List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer=new List<SkinnedMeshRenderer>();

	public MeshDetails()
	{

	}
};

public class ResourceChecker {


	static string[] inspectToolbarStrings = {"Textures", "Materials","Meshes"};

	enum InspectType 
	{
		Textures,Materials,Meshes
	};

	static bool IncludeDisabledObjects=false;
	static bool IncludeSpriteAnimations=true;
	static bool IncludeScriptReferences=true;
	static bool IncludeGuiElements=true;

	static InspectType ActiveInspectType=InspectType.Textures;

	//static float ThumbnailWidth=40;
	//static float ThumbnailHeight=40;

	public static List<TextureDetails> ActiveTextures=new List<TextureDetails>();
	public static List<MaterialDetails> ActiveMaterials=new List<MaterialDetails>();
	public static List<MeshDetails> ActiveMeshDetails=new List<MeshDetails>();

	//static Vector2 textureListScrollPos=new Vector2(0,0);
	//static Vector2 materialListScrollPos=new Vector2(0,0);
	//static Vector2 meshListScrollPos=new Vector2(0,0);

	static int TotalTextureMemory=0;
	static int TotalMeshVertices=0;

	//static bool ctrlPressed=false;

	//static int MinWidth=455;

	static bool collectedInPlayingMode;


	public static void DrawGUI() {
		IncludeDisabledObjects = GUILayout.Toggle(IncludeDisabledObjects, "Include disabled and internal objects");
		IncludeSpriteAnimations = GUILayout.Toggle(IncludeSpriteAnimations, "Look in sprite animations");
		IncludeScriptReferences = GUILayout.Toggle(IncludeScriptReferences, "Look in behavior fields");
		IncludeGuiElements = GUILayout.Toggle(IncludeGuiElements, "Look in GUI elements");
		if (GUILayout.Button("Refresh")) CheckResources();

		RemoveDestroyedResources();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Materials "+ActiveMaterials.Count);
		GUILayout.Label("Textures "+ActiveTextures.Count+" - "+FormatSizeString(TotalTextureMemory));
		GUILayout.Label("Meshes "+ActiveMeshDetails.Count+" - "+TotalMeshVertices+" verts");
		GUILayout.EndHorizontal();
		ActiveInspectType=(InspectType)GUILayout.Toolbar((int)ActiveInspectType,inspectToolbarStrings);

		//ctrlPressed=Event.current.control || Event.current.command;

		switch (ActiveInspectType)
		{
		case InspectType.Textures:
			ListTextures();
			break;
		case InspectType.Materials:
			ListMaterials();
			break;
		case InspectType.Meshes:
			ListMeshes();
			break;	


		}
	}

	private static void RemoveDestroyedResources()
	{
		if (collectedInPlayingMode != Application.isPlaying)
		{
			ActiveTextures.Clear();
			ActiveMaterials.Clear();
			ActiveMeshDetails.Clear();
			collectedInPlayingMode = Application.isPlaying;
		}

		ActiveTextures.RemoveAll(x => !x.texture);
		ActiveTextures.ForEach(delegate(TextureDetails obj) {
			obj.FoundInAnimators.RemoveAll(x => !x);
			obj.FoundInMaterials.RemoveAll(x => !x);
			obj.FoundInRenderers.RemoveAll(x => !x);
			obj.FoundInScripts.RemoveAll(x => !x);
			obj.FoundInGraphics.RemoveAll(x => !x);
		});

		ActiveMaterials.RemoveAll(x => !x.material);
		ActiveMaterials.ForEach(delegate(MaterialDetails obj) {
			obj.FoundInRenderers.RemoveAll(x => !x);
			obj.FoundInGraphics.RemoveAll(x => !x);
		});

		ActiveMeshDetails.RemoveAll(x => !x.mesh);
		ActiveMeshDetails.ForEach(delegate(MeshDetails obj) {
			obj.FoundInMeshFilters.RemoveAll(x => !x);
			obj.FoundInSkinnedMeshRenderer.RemoveAll(x => !x);
		});

		TotalTextureMemory = 0;
		foreach (TextureDetails tTextureDetails in ActiveTextures) TotalTextureMemory += tTextureDetails.memSizeKB;

		TotalMeshVertices = 0;
		foreach (MeshDetails tMeshDetails in ActiveMeshDetails) TotalMeshVertices += tMeshDetails.mesh.vertexCount;
	}

	static int  GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8: //	 Alpha-only texture format.
			return 8;
		case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
			return 16;
		case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
			return 16;
		case TextureFormat.RGB24:	// A color texture format.
			return 24;
		case TextureFormat.RGBA32:	//Color with an alpha channel texture format.
			return 32;
		case TextureFormat.ARGB32:	//Color with an alpha channel texture format.
			return 32;
		case TextureFormat.RGB565:	//	 A 16 bit color texture format.
			return 16;
		case TextureFormat.DXT1:	// Compressed color texture format.
			return 4;
		case TextureFormat.DXT5:	// Compressed color with alpha channel texture format.
			return 8;
			/*
			case TextureFormat.WiiI4:	// Wii texture format.
			case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
			case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
			case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
			case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
			case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
			case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
			case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
				return 0;  //Not supported yet
			*/
		case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
			return 2;
		case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
			return 2;
		case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
			return 4;
		case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
			return 4;
		case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
			return 4;
		case TextureFormat.ATC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
			return 4;
		case TextureFormat.ATC_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
			return 8;
		case TextureFormat.BGRA32://	 Format returned by iPhone camera
			return 32;
			#if !UNITY_5
			case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
			case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
			case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
			return 0; //Not supported yet  
			#endif
		}
		return 0;
	}

	static int CalculateTextureSizeBytes(Texture tTexture)
	{

		int tWidth=tTexture.width;
		int tHeight=tTexture.height;
		if (tTexture is Texture2D)
		{
			Texture2D tTex2D=tTexture as Texture2D;
			int bitsPerPixel=GetBitsPerPixel(tTex2D.format);
			int mipMapCount=tTex2D.mipmapCount;
			int mipLevel=1;
			int tSize=0;
			while (mipLevel<=mipMapCount)
			{
				tSize+=tWidth*tHeight*bitsPerPixel/8;
				tWidth=tWidth/2;
				tHeight=tHeight/2;
				mipLevel++;
			}
			return tSize;
		}

		if (tTexture is Cubemap)
		{
			Cubemap tCubemap=tTexture as Cubemap;
			int bitsPerPixel=GetBitsPerPixel(tCubemap.format);
			return tWidth*tHeight*6*bitsPerPixel/8;
		}
		return 0;
	}


	static void SelectObject(Object selectedObject,bool append)
	{
	}

	static void SelectObjects(List<Object> selectedObjects,bool append)
	{

	}

	static void ListTextures()
	{
		/*textureListScrollPos = EditorGUILayout.BeginScrollView(textureListScrollPos);

		foreach (TextureDetails tDetails in ActiveTextures)
		{			

			GUILayout.BeginHorizontal ();
			GUILayout.Box(tDetails.texture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));

			if(GUILayout.Button(tDetails.texture.name,GUILayout.Width(150)))
			{
				SelectObject(tDetails.texture,ctrlPressed);
			}

			string sizeLabel=""+tDetails.texture.width+"x"+tDetails.texture.height;
			if (tDetails.isCubeMap) sizeLabel+="x6";
			sizeLabel+=" - "+tDetails.mipMapCount+"mip";
			sizeLabel+="\n"+FormatSizeString(tDetails.memSizeKB)+" - "+tDetails.format+"";

			GUILayout.Label (sizeLabel,GUILayout.Width(120));

			if(GUILayout.Button(tDetails.FoundInMaterials.Count+" Mat",GUILayout.Width(50)))
			{
				SelectObjects(tDetails.FoundInMaterials,ctrlPressed);
			}

			HashSet<Object> FoundObjects = new HashSet<Object>();
			foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
			foreach (Animator animator in tDetails.FoundInAnimators) FoundObjects.Add(animator.gameObject);
			foreach (Graphic graphic in tDetails.FoundInGraphics) FoundObjects.Add(graphic.gameObject);
			foreach (MonoBehaviour script in tDetails.FoundInScripts) FoundObjects.Add(script.gameObject);
			if (GUILayout.Button(FoundObjects.Count+" GO",GUILayout.Width(50)))
			{
				SelectObjects(new List<Object>(FoundObjects),ctrlPressed);
			}

			GUILayout.EndHorizontal();	
		}
		if (ActiveTextures.Count>0)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Box(" ",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));

			if(GUILayout.Button("Select All",GUILayout.Width(150)))
			{
				List<Object> AllTextures=new List<Object>();
				foreach (TextureDetails tDetails in ActiveTextures) AllTextures.Add(tDetails.texture);
				SelectObjects(AllTextures,ctrlPressed);
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();*/
	}

	static void ListMaterials()
	{
		/*materialListScrollPos = EditorGUILayout.BeginScrollView(materialListScrollPos);

		foreach (MaterialDetails tDetails in ActiveMaterials)
		{			
			if (tDetails.material!=null)
			{
				GUILayout.BeginHorizontal ();

				if (tDetails.material.mainTexture!=null) GUILayout.Box(tDetails.material.mainTexture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
				else	
				{
					GUILayout.Box("n/a",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
				}

				if(GUILayout.Button(tDetails.material.name,GUILayout.Width(150)))
				{
					SelectObject(tDetails.material,ctrlPressed);
				}

				string shaderLabel = tDetails.material.shader != null ? tDetails.material.shader.name : "no shader";
				GUILayout.Label (shaderLabel, GUILayout.Width(200));

				if(GUILayout.Button((tDetails.FoundInRenderers.Count + tDetails.FoundInGraphics.Count) +" GO",GUILayout.Width(50)))
				{
					List<Object> FoundObjects=new List<Object>();
					foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
					foreach (Graphic graphic in tDetails.FoundInGraphics) FoundObjects.Add(graphic.gameObject);
					SelectObjects(FoundObjects,ctrlPressed);
				}


				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();	*/	
	}

	static void ListMeshes()
	{
		/*meshListScrollPos = EditorGUILayout.BeginScrollView(meshListScrollPos);

		foreach (MeshDetails tDetails in ActiveMeshDetails)
		{			
			if (tDetails.mesh!=null)
			{
				GUILayout.BeginHorizontal ();

				if(GUILayout.Button(tDetails.mesh.name,GUILayout.Width(150)))
				{
					SelectObject(tDetails.mesh,ctrlPressed);
				}
				string sizeLabel=""+tDetails.mesh.vertexCount+" vert";

				GUILayout.Label (sizeLabel,GUILayout.Width(100));


				if(GUILayout.Button(tDetails.FoundInMeshFilters.Count + " GO",GUILayout.Width(50)))
				{
					List<Object> FoundObjects=new List<Object>();
					foreach (MeshFilter meshFilter in tDetails.FoundInMeshFilters) FoundObjects.Add(meshFilter.gameObject);
					SelectObjects(FoundObjects,ctrlPressed);
				}

				if(GUILayout.Button(tDetails.FoundInSkinnedMeshRenderer.Count + " GO",GUILayout.Width(50)))
				{
					List<Object> FoundObjects=new List<Object>();
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in tDetails.FoundInSkinnedMeshRenderer) FoundObjects.Add(skinnedMeshRenderer.gameObject);
					SelectObjects(FoundObjects,ctrlPressed);
				}


				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();	*/	
	}

	static string FormatSizeString(int memSizeKB)
	{
		if (memSizeKB<1024) return ""+memSizeKB+"k";
		else
		{
			float memSizeMB=((float)memSizeKB)/1024.0f;
			return memSizeMB.ToString("0.00")+"Mb";
		}
	}


	static TextureDetails FindTextureDetails(Texture tTexture)
	{
		foreach (TextureDetails tTextureDetails in ActiveTextures)
		{
			if (tTextureDetails.texture==tTexture) return tTextureDetails;
		}
		return null;

	}

	static MaterialDetails FindMaterialDetails(Material tMaterial)
	{
		foreach (MaterialDetails tMaterialDetails in ActiveMaterials)
		{
			if (tMaterialDetails.material==tMaterial) return tMaterialDetails;
		}
		return null;

	}

	static MeshDetails FindMeshDetails(Mesh tMesh)
	{
		foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
		{
			if (tMeshDetails.mesh==tMesh) return tMeshDetails;
		}
		return null;

	}


	public static void CheckResources()
	{
		ActiveTextures.Clear();
		ActiveMaterials.Clear();
		ActiveMeshDetails.Clear();


		var texset = Resources.FindObjectsOfTypeAll<Texture2D>();
		foreach( var tex in texset ) {
			var td = new TextureDetails();
			td.texture = tex;
			int bmul = (tex.format == TextureFormat.ARGB32 || tex.format == TextureFormat.RGBA32 ) ? 4 : 3;
			td.memSizeKB = ( tex.width * tex.height * bmul ) / 1024 ;
			ActiveTextures.Add( td );
		}



	}

	static private T[] FindObjects<T>() where T : Object
	{
		if (IncludeDisabledObjects)
		{
			return Resources.FindObjectsOfTypeAll<T>();
		}
		else
		{
			return (T[])Object.FindObjectsOfType(typeof(T));
		}
	}

	static private TextureDetails GetTextureDetail(Texture tTexture, Material tMaterial, MaterialDetails tMaterialDetails)
	{
		TextureDetails tTextureDetails = GetTextureDetail(tTexture);

		tTextureDetails.FoundInMaterials.Add(tMaterial);
		foreach (Renderer renderer in tMaterialDetails.FoundInRenderers)
		{
			if (!tTextureDetails.FoundInRenderers.Contains(renderer)) tTextureDetails.FoundInRenderers.Add(renderer);
		}
		return tTextureDetails;
	}

	static private TextureDetails GetTextureDetail(Texture tTexture, Renderer renderer)
	{
		TextureDetails tTextureDetails = GetTextureDetail(tTexture);

		tTextureDetails.FoundInRenderers.Add(renderer);
		return tTextureDetails;
	}

	static private TextureDetails GetTextureDetail(Texture tTexture, Animator animator)
	{
		TextureDetails tTextureDetails = GetTextureDetail(tTexture);

		tTextureDetails.FoundInAnimators.Add(animator);
		return tTextureDetails;
	}

	static private TextureDetails GetTextureDetail(Texture tTexture, Graphic graphic)
	{
		TextureDetails tTextureDetails = GetTextureDetail(tTexture);

		tTextureDetails.FoundInGraphics.Add(graphic);
		return tTextureDetails;
	}

	static private TextureDetails GetTextureDetail(Texture tTexture, MonoBehaviour script)
	{
		TextureDetails tTextureDetails = GetTextureDetail(tTexture);

		tTextureDetails.FoundInScripts.Add(script);
		return tTextureDetails;
	}

	static private TextureDetails GetTextureDetail(Texture tTexture)
	{
		TextureDetails tTextureDetails = FindTextureDetails(tTexture);
		if (tTextureDetails == null)
		{
			tTextureDetails = new TextureDetails();
			tTextureDetails.texture = tTexture;
			tTextureDetails.isCubeMap = tTexture is Cubemap;

			int memSize = CalculateTextureSizeBytes(tTexture);

			tTextureDetails.memSizeKB = memSize / 1024;
			TextureFormat tFormat = TextureFormat.RGBA32;
			int tMipMapCount = 1;
			if (tTexture is Texture2D)
			{
				tFormat = (tTexture as Texture2D).format;
				tMipMapCount = (tTexture as Texture2D).mipmapCount;
			}
			if (tTexture is Cubemap)
			{
				tFormat = (tTexture as Cubemap).format;
			}

			tTextureDetails.format = tFormat;
			tTextureDetails.mipMapCount = tMipMapCount;

		}

		return tTextureDetails;
	}

}
