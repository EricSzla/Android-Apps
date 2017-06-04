package erpam.pokequiz;

import com.google.android.gms.ads.AdListener;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdView;
import com.google.android.gms.ads.InterstitialAd;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageView;
import android.widget.ProgressBar;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Timer;
import java.util.TimerTask;

public class SplashAd extends AppCompatActivity {
    // Remove the below line after defining your own ad unit ID.
    private static final String TOAST_TEXT = "Test ads are being shown. "
            + "To show live ads, replace the ad unit ID in res/values/strings.xml with your own ad unit ID.";

    private static final int START_LEVEL = 1;
    private int mLevel;
    private InterstitialAd mInterstitial;
    private Timer waitTimer;
    private boolean interstitialCanceled;
    private Bundle bundle;
    private AdView adView;
    private ProgressBar progressBar;
    private String TAG = "SplashAd";

    private ArrayList<HashMap<String,String>> pokemonArray;
    private int level = 0;
    private int pokemon_no = 0;
    int lives = 0;
    boolean docontinue = false;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        ImageView image = new ImageView(this);
        // Assumes you have a resource with the name kitten.
        image.setImageResource(R.drawable.pokeball_by_erykszlachetka);
        setContentView(R.layout.activity_splash_ad);
        ActionBar actionBar = getSupportActionBar();
        if(actionBar != null) {
            actionBar.hide();
        }


        bundle = getIntent().getExtras();
        pokemonArray = (ArrayList<HashMap<String,String>>) bundle.getSerializable("array");
        level = bundle.getInt("level");
        pokemon_no = bundle.getInt("pokeNo");
        lives = bundle.getInt("lives");
        docontinue = bundle.getBoolean("docontinue");

        progressBar = (ProgressBar)findViewById(R.id.progressBar);
        progressBar.setVisibility(View.VISIBLE);
        adView = (AdView) findViewById(R.id.adViewLead);
        adView.setVisibility(View.INVISIBLE);
        AdRequest adRequest = new AdRequest.Builder().build();


        mInterstitial = new InterstitialAd(this);
        mInterstitial.setAdUnitId(getString(R.string.interstitial_ad_unit_id));
        mInterstitial.loadAd(adRequest);
        mInterstitial.setAdListener(new AdListener() {
            @Override
            public void onAdLoaded() {
                progressBar.setVisibility(View.GONE);
                adView.setVisibility(View.VISIBLE);
                if (!interstitialCanceled) {
                    waitTimer.cancel();
                    mInterstitial.show();
                }
            }

            @Override
            public void onAdFailedToLoad(int errorCode) {
                startMainActivity();
            }

            @Override
            public void onAdClosed() {
                //super.onAdClosed();
                Log.i("Ad","closed");
            }
        });

        waitTimer = new Timer();
        waitTimer.schedule(new TimerTask() {
            @Override
            public void run() {
                interstitialCanceled = true;
                SplashAd.this.runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        startMainActivity();
                    }
                });
            }
        }, 100000);


    }

    @Override
    public void onPause() {
        waitTimer.cancel();
        interstitialCanceled = true;
        super.onPause();
    }

    @Override
    public void onResume() {
        super.onResume();
        if (mInterstitial.isLoaded()) {
            mInterstitial.show();
        } else if (interstitialCanceled) {
            startMainActivity();
        }
    }

    private void startMainActivity() {
        Intent intent = new Intent(this, ThirdMenu.class);
        Bundle extras = new Bundle();
        extras.putSerializable("array",pokemonArray);
        extras.putInt("level",level);
        extras.putInt("lives",lives);
        extras.putInt("position",pokemon_no);
        extras.putBoolean("docontinue",true);
        extras.putBoolean("adsLeft",true);
        intent.putExtras(extras);
        startActivity(intent);
        finish();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_splash_ad, menu);
        return true;
    }

    @Override
    public void onBackPressed() {
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }
}
