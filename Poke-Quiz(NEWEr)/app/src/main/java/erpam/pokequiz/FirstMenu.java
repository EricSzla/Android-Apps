package erpam.pokequiz;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.Toast;

import java.util.ArrayList;

/**
 * Created by Eryk on 23/07/16.
 */
public class FirstMenu extends AppCompatActivity {

    private ListView listView;
    private MenuAdapter menuAdapter;
    private PreUtil preUtil;
    private int allowBonus = 0;
    private Boolean adsLeft = false;
    private static Activity activity;
    private Boolean later = false;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_firstmenu);

        adsLeft = getIntent().getBooleanExtra("adsLeft", false);
        later = getIntent().getBooleanExtra("later", false);


        activity = this;
        preUtil = new PreUtil(this);
        //preUtil.clearValues();
        //preUtil.saveLevel("5");
        //preUtil.savePokemonNum("1");
        String bonus = preUtil.getBonusStatus();
        if (bonus == null || bonus.equals("0")) {
            allowBonus = 0;
        } else {
            allowBonus = 1;
        }

        int level2 = 0;
        if (preUtil.getLevel() != null) {
            level2 = Integer.valueOf(preUtil.getLevel());
        } else {
            level2 = 1;
        }
        int pokemon_num = 0;
        if (preUtil.getPokemonNumber() != null) {
            pokemon_num = Integer.valueOf(preUtil.getPokemonNumber());
        } else {
            pokemon_num = 0;
        }
        int lives = 0;
        if (preUtil.getLives() != null) {
            lives = Integer.valueOf(preUtil.getLives());
        } else {
            lives = 3;
        }
        final int level = level2;
        final int pokemonNo = pokemon_num;
        final int pokeLives = lives;

        if (level % 2 == 0 && !later) {
            String isRated = preUtil.getBonusStatus();
            if (isRated != null) {
                if (!isRated.equals("1")) {
                    RateMeMaybe rmm = new RateMeMaybe(this);
                    rmm.resetData(this);
                    rmm.setPromptMinimums(1, 0, 1, 0);
                    rmm.setDialogMessage("It would be great if you took a moment to rate our app ! "
                            + "Without rating it is impossible to get the BONUS !");
                    rmm.setDialogTitle("Rate " + getResources().getString(R.string.app_name));
                    rmm.setPositiveBtn("Give me that bonus!");
                    rmm.setNeutralBtn("Later");
                    rmm.run();
                }
            } else {
                preUtil.saveBonusStatus("0");
                RateMeMaybe rmm = new RateMeMaybe(this);
                rmm.resetData(this);
                rmm.setPromptMinimums(1, 0, 1, 0);
                rmm.setDialogMessage("It would be great if you took a moment to rate our app ! "
                        + "Without rating it is impossible to get the BONUS !");
                rmm.setDialogTitle("Rate " + getResources().getString(R.string.app_name));
                rmm.setPositiveBtn("Give me that bonus!");
                rmm.setNeutralBtn("Later");
                rmm.run();
            }
        }

        ArrayList<String> titles = new ArrayList<String>();
        titles.add(0, "Stage One");
        titles.add(1, "Level 1");
        titles.add(2, "Level 2");
        titles.add(3, "Level 3");
        titles.add(4, "Stage Two");
        titles.add(5, "Level 1");
        titles.add(6, "Level 2");
        titles.add(7, "Level 3");
        titles.add(8, "Bonus!");
        titles.add(9, "Go !");

        int[] ids = new int[10];
        ids[0] = R.drawable.pokeball_by_erykszlachetka;
        for (int i = 1; i < 4; i++) {
            if (level < i) {
                ids[i] = R.drawable.locked;
            } else {
                ids[i] = R.drawable.pokeball_by_erykszlachetka;
            }
        }
        ids[4] = R.drawable.pokeball_by_erykszlachetka;
        for (int i = 5; i < 8; i++) {
            if (level < i) {
                ids[i] = R.drawable.locked;
            } else {
                ids[i] = R.drawable.pokeball_by_erykszlachetka;
            }
        }
        ids[8] = R.drawable.pokeball_by_erykszlachetka;
        if (level < 9) {
            ids[9] = R.drawable.locked;
        } else {
            ids[9] = R.drawable.pokeball_by_erykszlachetka;
        }

        listView = (ListView) findViewById(R.id.firstMenu_listView);
        menuAdapter = new MenuAdapter(this, titles, ids);
        listView.setAdapter(menuAdapter);
        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                if (position <= level) {
                    if (position != 0 && position != 4 && position != 8) {
                        if (position != 9 || allowBonus == 1) {
                            Intent i = new Intent(getApplicationContext(), SecondMenu.class);
                            Bundle bundle = new Bundle();
                            bundle.putInt("position", position);
                            bundle.putInt("lives", pokeLives);
                            if (position == level) {
                                bundle.putInt("pokeNo", pokemonNo);
                                bundle.putBoolean("continue", true);
                            } else {
                                bundle.putInt("pokeNo", 20);
                                Toast.makeText(FirstMenu.this, "Already finished", Toast.LENGTH_SHORT).show();
                                bundle.putBoolean("continue", false);
                            }
                            bundle.putBoolean("adsLeft", adsLeft);
                            i.putExtras(bundle);
                            startActivity(i);
                        } else if (allowBonus == 0) {
                            RateMeMaybe rmm = new RateMeMaybe(FirstMenu.this);
                            rmm.resetData(FirstMenu.this);
                            rmm.setPromptMinimums(1, 0, 1, 0);
                            rmm.setDialogMessage("It would be great if you took a moment to rate our app ! "
                                    + "Without rating it is impossible to get the BONUS !");
                            rmm.setDialogTitle("Rate " + getResources().getString(R.string.app_name));
                            rmm.setPositiveBtn("Give me that bonus!");
                            rmm.setNeutralBtn("Later");
                            rmm.run();
                        }
                    }
                } else if (position > level && position == 9) {
                    Toast.makeText(FirstMenu.this, "Finish all the stages first!", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(FirstMenu.this, "Finish level " + level + " first !", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    public static Activity getActivityThis() {
        return activity;
    }
}
