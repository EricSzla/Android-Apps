package erpam.pokequiz;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdView;

public class MainActivity extends AppCompatActivity {

    public static PreUtil preUtil;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        preUtil = new PreUtil(this);
        AdView adView = (AdView) findViewById(R.id.adView);
        AdRequest adRequest = new AdRequest.Builder().build();
        if (adView != null) {
            adView.loadAd(adRequest);
        }

        ImageView pokeBall = (ImageView) findViewById(R.id.pokeBallView);
        if (pokeBall != null) {
            pokeBall.animate().alpha(1).setDuration(3000).start();

            pokeBall.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    Intent i = new Intent(getApplicationContext(), FirstMenu.class);
                    startActivity(i);
                }
            });
        }

        TextView textView = (TextView) findViewById(R.id.textView);
        if (textView != null) {
            textView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    Intent i = new Intent(getApplicationContext(), FirstMenu.class);
                    startActivity(i);
                }
            });
        }

        if(preUtil.getFinishedValue() != null) {
            String finished = preUtil.getFinishedValue();
            if (finished.equals("true")) {
                final Button button = (Button) findViewById(R.id.reset_button);
                if (button != null) {
                    button.setVisibility(View.VISIBLE);
                    button.setOnClickListener(new View.OnClickListener() {
                        @Override
                        public void onClick(View v) {
                            preUtil.clearValues();
                            Toast.makeText(MainActivity.this, "Done", Toast.LENGTH_SHORT).show();
                            preUtil.saveFinishedValue("false");
                            button.setVisibility(View.GONE);
                            Intent i = new Intent(getApplicationContext(),MainActivity.class);
                            startActivity(i);
                        }
                    });
                }
            }
        }
    }
}

