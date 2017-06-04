package erpam.pokequiz;

/**
 * Created by Eryk on 24/07/16.
 */

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.GridView;
import android.widget.ImageView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;

import java.util.ArrayList;
import java.util.HashMap;

public class ImageAdapter extends BaseAdapter {
    private Context mContext;
    private ArrayList<HashMap<String, String>> pokemonArrayList = new ArrayList<>();
    private Integer[] mThumbIds;
    private Integer[] mShadowThumbIds;
    public static final String TAG_ID = "id";
    public static final String TAG_SHADOW_ID = "shadowid";
    private int pokemonNo;

    // Constructor
    public ImageAdapter(Context c, ArrayList<HashMap<String, String>> arrayList, int pn) {
        mContext = c;
        this.pokemonArrayList = arrayList;
        this.pokemonNo = pn;
        setupmThumbIds();
    }

    private void setupmThumbIds() {
        mThumbIds = new Integer[pokemonArrayList.size()];
        mShadowThumbIds = new Integer[pokemonArrayList.size()];
        for (int i = 0; i < pokemonArrayList.size(); i++) {
            mThumbIds[i] = Integer.valueOf(pokemonArrayList.get(i).get(TAG_ID));
            mShadowThumbIds[i] = Integer.valueOf(pokemonArrayList.get(i).get(TAG_SHADOW_ID));
        }
    }

    @Override
    public int getCount() {
        return mThumbIds.length;
    }

    @Override
    public Object getItem(int position) {
        return mThumbIds[position];
    }

    @Override
    public long getItemId(int position) {
        return 0;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent){
        ImageView imageView = new ImageView(mContext);
        imageView.setLayoutParams(new GridView.LayoutParams(200, 200));
        if(position < pokemonNo) {
            glide(mThumbIds[position], imageView);
        }else if(position == pokemonNo){
            glide(R.drawable.pokeball_by_erykszlachetka,imageView);
        } else{
            glide(mShadowThumbIds[position],imageView);
        }
        return imageView;
    }


    private void glide(int id, ImageView imageView){
        Glide.with(mContext)
                .load(id)
                //.placeholder(R.drawable.ic_perm_identity_white_24dp)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .fitCenter()
                .override(70,70)
                .into(imageView);
    }

}