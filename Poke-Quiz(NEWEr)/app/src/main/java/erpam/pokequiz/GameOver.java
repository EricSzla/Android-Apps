package erpam.pokequiz;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.HashMap;

public class GameOver extends AppCompatActivity {

    Bundle bundle;
    private ArrayList<HashMap<String,String>> pokemonArray;
    private int level = 0;
    private int pokemon_no = 0;
    int lives = 0;
    boolean docontinue = false;
    boolean afterAd = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game_over);
        bundle = getIntent().getExtras();
        pokemonArray = (ArrayList<HashMap<String,String>>) bundle.getSerializable("array");
        level = bundle.getInt("level");
        pokemon_no = bundle.getInt("pokeNo");
        lives = bundle.getInt("lives");
        docontinue = bundle.getBoolean("docontinue");
        afterAd = bundle.getBoolean("adsLeft");

        if(afterAd){
            TextView textView = (TextView) findViewById(R.id.textView3);
            textView.setText("You can only watch the ad once !");
            Button button = (Button) findViewById(R.id.button2);
            if(button != null) {
                button.setVisibility(View.INVISIBLE);
            }
        }

    }

    @Override
    public void onBackPressed() {
    }

    public void watchAd(View view){
        Intent i = new Intent(getApplicationContext(),SplashAd.class);
        Bundle extras = new Bundle();
        extras.putSerializable("array",pokemonArray);
        extras.putInt("level",level);
        extras.putInt("lives",lives);
        extras.putInt("pokeNo",pokemon_no);
        extras.putBoolean("docontinue",docontinue);
        extras.putBoolean("adsLeft",false);
        i.putExtras(extras);
        startActivity(i);
    }

    public void startAgain(View view){
        PreUtil preUtil = new PreUtil(this);
        /*preUtil.saveLevel("1");
        preUtil.savePokemonNum("0");
        preUtil.saveLives("3");*/
        preUtil.clearValues();
        Intent i = new Intent(getApplicationContext(),FirstMenu.class);
        startActivity(i);

        /*Intent i = new Intent(getApplicationContext(),SecondMenu.class);
        Bundle extras = new Bundle();
        extras.putSerializable("array",pokemonArray);
        extras.putInt("level",level);
        extras.putInt("lives",3);
        extras.putInt("pokeNo",0);
        extras.putBoolean("continue",docontinue);
        afterAd = !afterAd;
        extras.putBoolean("adsLeft", afterAd);
        i.putExtras(extras);
        startActivity(i);*/
    }
}
