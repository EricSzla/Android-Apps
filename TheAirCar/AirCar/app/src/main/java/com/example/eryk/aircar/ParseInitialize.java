package com.example.eryk.aircar;

import android.app.Application;
import android.util.Log;

import com.parse.Parse;

/**
 * Created by Eryk on 18/11/16.
 * Class used to initialize the Parse database.
 */

public class ParseInitialize extends Application {

    @Override
    public void onCreate() {
        super.onCreate();
        Parse.enableLocalDatastore(this);

        Parse.initialize(new Parse.Configuration.Builder(getApplicationContext())
                .applicationId("aircar28521Wdsaj42")
                .clientKey(null)
                .server("http://aircar.herokuapp.com/parse/")
                .build()
        );

        Log.i("Main"," Initialized");
    }
}
