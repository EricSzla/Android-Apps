package erpam.pokequiz;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;

public class TheEnd extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_the_end);
    }


    public void goBack(View view){
        Intent i = new Intent(getApplicationContext(),MainActivity.class);
        PreUtil preUtil = new PreUtil(this);
        preUtil.saveFinishedValue("true");
        startActivity(i);
    }

    @Override
    public void onBackPressed() {
    }
}
