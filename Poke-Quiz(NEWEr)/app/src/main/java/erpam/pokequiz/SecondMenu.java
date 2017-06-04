package erpam.pokequiz;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.GridView;
import android.widget.Toast;

import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdView;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.HashMap;

/**
 * Created by pamela on 23/07/2016.
 */
public class SecondMenu extends AppCompatActivity {

    ArrayList<HashMap<String, String>> pokemonArray = new ArrayList<HashMap<String, String>>();
    int level = 0;
    int pokeNo = 0;
    int lives = 3;
    Boolean docontinue = false;
    Boolean adsLeft = false;

    public static final String TAG_NAME = "name";
    public static final String TAG_ID = "id";
    public static final String TAG_SHADOW_ID = "shadowid";

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.grid_layout);

        Intent intent = getIntent();
        level = intent.getIntExtra("position", 1);
        pokeNo = intent.getIntExtra("pokeNo", 1);
        lives = intent.getIntExtra("lives", 3);
        docontinue = intent.getBooleanExtra("continue",false);
        adsLeft = intent.getBooleanExtra("adsLeft",false);

        AdView adView = (AdView) findViewById(R.id.adViewGrid);
        AdRequest adRequest = new AdRequest.Builder().build();
        if (adView != null) {
            adView.loadAd(adRequest);
        }

        R.drawable drawableResources = new R.drawable();
        Class<R.drawable> c = R.drawable.class;
        Field[] fields = c.getDeclaredFields();
        int resourceId;
        Log.i("Fields", String.valueOf(fields.length));
        Log.i("Level", String.valueOf(level));
        for (int i = 0; i < fields.length; i++) {
            try {
                resourceId = fields[i].getInt(drawableResources);
            } catch (Exception e) {
                continue;
            }
            if ((level == 1 && getResources().getResourceEntryName(resourceId).startsWith("_l1_1")) ||
                    (level == 2 && getResources().getResourceEntryName(resourceId).startsWith("_l1_2")) ||
                    (level == 3 && getResources().getResourceEntryName(resourceId).startsWith("_l1_3")) ||
                    (level == 5 && getResources().getResourceEntryName(resourceId).startsWith("_l2_1")) ||
                    (level == 6 && getResources().getResourceEntryName(resourceId).startsWith("_l2_2")) ||
                    (level == 7 && getResources().getResourceEntryName(resourceId).startsWith("_l2_3")) ||
                    (level == 9 && getResources().getResourceEntryName(resourceId).startsWith("_l3_1"))) {

                Log.i("hi", getResources().getResourceEntryName(resourceId));
                HashMap<String, String> hashMap = new HashMap<String,String>();
                String prefix = "";

                if (level == 1) {
                    prefix = "_l1_1";
                } else if (level == 2) {
                    prefix = "_l1_2";
                } else if (level == 3) {
                    prefix = "_l1_3";
                } else if (level == 5) {
                    prefix = "_l2_1";
                } else if (level == 6) {
                    prefix = "_l2_2";
                } else if (level == 7) {
                    prefix = "_l2_3";
                } else if (level == 9) {
                    prefix = "_l3_1";
                }
                if (getResources().getResourceEntryName(resourceId).endsWith("2")) {
                    if(getResources().getResourceEntryName(resourceId).startsWith("_l1_2nidoran")){
                        hashMap.put(TAG_NAME, getResources().getResourceEntryName(resourceId).replace(getResources().getResourceEntryName(resourceId), "nidoran"));
                    }else {
                        hashMap.put(TAG_NAME, getResources().getResourceEntryName(resourceId).replace(prefix, "").replace("2", ""));
                    }
                    hashMap.put(TAG_ID, String.valueOf(resourceId - 1));
                    hashMap.put(TAG_SHADOW_ID, String.valueOf(resourceId));
                    pokemonArray.add(hashMap);
                }

            }
        }

        GridView gridLayout = (GridView) findViewById(R.id.grid_view);
        if (gridLayout != null) {
            ImageAdapter adapter = new ImageAdapter(this, pokemonArray, pokeNo);
            gridLayout.setAdapter(adapter);

            gridLayout.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

                    if (position <= pokeNo) {
                        Intent i = new Intent(getApplicationContext(), ThirdMenu.class);
                        Bundle bundle = new Bundle();
                        bundle.putSerializable("array", pokemonArray);
                        bundle.putInt("position", position);
                        bundle.putInt("level", level);
                        bundle.putInt("lives", lives);
                        if(docontinue) {
                            if(position == pokeNo) {
                                bundle.putBoolean("docontinue", true);
                            }else if(position < pokeNo){
                                bundle.putBoolean("docontinue", false);
                            }
                        }else{
                            bundle.putBoolean("docontinue",false);
                        }
                        bundle.putBoolean("adsLeft",adsLeft);
                        i.putExtras(bundle);
                        startActivity(i);

                    } else {
                        Toast.makeText(SecondMenu.this, "Pokemon not available yet!", Toast.LENGTH_SHORT).show();
                    }
                }
            });
        }
    }

    @Override
    public void onBackPressed() {
        Intent i = new Intent(getApplicationContext(),FirstMenu.class);
        i.putExtra("adsLeft",adsLeft);
        startActivity(i);
    }
}
