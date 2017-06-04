package erpam.pokequiz;

import android.app.Activity;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;

/**
 * Created by Eryk on 01/07/16.
 */
public class PreUtil {
    private static Activity activity;
    private final static String TAG_LEVEL = "level";
    private final static String TAG_POKEMON_NUMBER = "pokemon_number";
    private final static String TAG_LIVES = "lives";
    private final static String TAG_BONUS = "bonus";
    private final static String TAG_FINISHED = "finished";



    // constructor
    public PreUtil(Activity activity) {
        this.activity = activity;
    }

    public void saveLevel(String level) {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_LEVEL, level);
        editor.apply();
    }

    public void savePokemonNum(String pokemon_number){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_POKEMON_NUMBER,pokemon_number);
        editor.apply();
    }

    public void saveLives(String lives){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_LIVES,lives);
        editor.apply();
    }

    public void saveFinishedValue(String value){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_FINISHED,value);
        editor.apply();
    }
    public void saveBonusStatus(String bonus){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_BONUS,bonus);
        editor.apply();
    }

    public String getLevel() {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        return sp.getString(TAG_LEVEL, null);
    }

    public String getFinishedValue() {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        return sp.getString(TAG_FINISHED, null);
    }

    public String getPokemonNumber(){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        return sp.getString(TAG_POKEMON_NUMBER,null);
    }

    public String getLives() {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        return sp.getString(TAG_LIVES, null);
    }

    public String getBonusStatus(){
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        return sp.getString(TAG_BONUS, null);
    }

    public void clearValues() {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(activity);
        SharedPreferences.Editor editor = sp.edit();
        editor.putString(TAG_LEVEL, null);
        editor.putString(TAG_POKEMON_NUMBER,null);
        editor.putString(TAG_LIVES,null);
        editor.putString(TAG_BONUS,null);
        editor.clear();
        editor.apply();
    }
}
