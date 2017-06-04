package erpam.pokequiz;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.text.InputType;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdView;

import java.util.ArrayList;
import java.util.HashMap;

/**
 * Created by pamela on 24/07/2016.
 */
public class ThirdMenu extends AppCompatActivity {

    public static final String TAG_NAME = "name";
    public static final String TAG_ID = "id";
    public static final String TAG_SHADOWID = "shadowid";
    int pokemon_number = 0;
    int lives = 0;
    int level = 0;
    Boolean docontinue = false;
    boolean shadow = true;
    boolean fadeFinished = false;
    ArrayList<HashMap<String, String>> pokemonArray = new ArrayList<>();
    PreUtil preUtil;
    boolean adsLeft = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_thirdmenu);
        ActionBar actionBar = getSupportActionBar();
        actionBar.setDisplayHomeAsUpEnabled(true);
        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_PAN);

        preUtil = new PreUtil(this);
        //Code for ads
        AdView adView = (AdView) findViewById(R.id.adView2);
        AdRequest adRequest = new AdRequest.Builder().build();
        if (adView != null) {
            adView.loadAd(adRequest);
        }

        Bundle bla = getIntent().getExtras();
        if (bla != null) {
            level = bla.getInt("level", 1);
            pokemonArray = (ArrayList<HashMap<String, String>>) bla.getSerializable("array");
            Log.i("PokemonArray", pokemonArray.toString());
            pokemon_number = bla.getInt("position", 0);
            Log.i("pokNo", String.valueOf(pokemon_number));
            lives = bla.getInt("lives", 3);
            docontinue = bla.getBoolean("docontinue", false);
            adsLeft = bla.getBoolean("adsLeft",false);
        } else {
            Log.i("Bla", "null");
        }
        //level = i.getIntExtra("position", 1);
        //pokemonArray = (ArrayList<HashMap<String,String>>) i.getSerializableExtra("array");
        //pokemon_number = i.getIntExtra("pokeNo", 0);
        //lives = i.getIntExtra("lives",3);
        if (docontinue) {
            compareStrings();
            setPokemonImage();
            setLives();
        } else {
            setPokemonImageWithoutPlay();
        }
    }

    public void setPokemonImageWithoutPlay() {
        final ImageView pokemon_imageView = (ImageView) findViewById(R.id.pokemon_imageView);
        TextView pokemon_textView = (TextView) findViewById(R.id.pokemon_textView);
        Button button = (Button) findViewById(R.id.button);
        EditText user_input = (EditText) findViewById(R.id.user_input);
        if (pokemon_imageView != null && pokemon_textView != null && user_input != null && button != null) {
            pokemon_imageView.setImageResource(Integer.parseInt(pokemonArray.get(pokemon_number).get(TAG_ID)));
            pokemon_textView.setText(pokemonArray.get(pokemon_number).get(TAG_NAME).substring(0,1).toUpperCase() + pokemonArray.get(pokemon_number).get(TAG_NAME).substring(1));
            button.setText("Go back");
            button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    Intent i = new Intent(getApplicationContext(),FirstMenu.class);
                    startActivity(i);
                }
            });
            user_input.setVisibility(View.INVISIBLE);

            pokemon_imageView.setAlpha(0f);
            pokemon_imageView.animate().alpha(1).setDuration(2000).start();

        }
    }

    public void setPokemonImage() {
        final ImageView pokemon_imageView = (ImageView) findViewById(R.id.pokemon_imageView);
        TextView pokemon_textView = (TextView) findViewById(R.id.pokemon_textView);
        Button button = (Button) findViewById(R.id.button);
        EditText user_input = (EditText) findViewById(R.id.user_input);
        if (pokemon_imageView != null && pokemon_textView != null && user_input != null && button != null) {
            if (shadow) {
                pokemon_imageView.setImageResource(Integer.parseInt(pokemonArray.get(pokemon_number).get(TAG_SHADOWID)));
                pokemon_textView.setText("Name of Pokemon");
                user_input.setVisibility(View.VISIBLE);
                user_input.requestFocus();

            } else {
                pokemon_imageView.setImageResource(Integer.parseInt(pokemonArray.get(pokemon_number).get(TAG_ID)));
                pokemon_textView.setText(pokemonArray.get(pokemon_number).get(TAG_NAME).substring(0,1).toUpperCase() + pokemonArray.get(pokemon_number).get(TAG_NAME).substring(1));
                button.setText("Next");
                user_input.setVisibility(View.INVISIBLE);

                pokemon_imageView.setAlpha(0f);
                pokemon_imageView.animate().alpha(1).setDuration(2000).start();
            }
        }
    }

    @Override
    public void onBackPressed() {
    }

    public void compareStrings() {
        Button button = (Button) findViewById(R.id.button);
        final EditText user_input = (EditText) findViewById(R.id.user_input);
        user_input.setInputType( InputType.TYPE_TEXT_VARIATION_URI );

        if (button != null) {
            button.setOnClickListener(new View.OnClickListener() {
                public void onClick(View view) {
                    inputEntered();
                }
            });
        } else {
            Log.i("Button", " null");
        }
        if (user_input != null) {
            user_input.setOnKeyListener(new View.OnKeyListener() {
                public boolean onKey(View v, int keyCode, KeyEvent event) {
                    if (event.getAction() == KeyEvent.ACTION_DOWN)
                        if ((keyCode == KeyEvent.KEYCODE_DPAD_CENTER) ||
                                (keyCode == KeyEvent.KEYCODE_ENTER)) {
                            //do something
                            inputEntered();
                            //true because you handle the event
                            return true;
                        }
                    return false;
                }
            });
        } else {
            Log.i("user_input", " null");
        }

        // kill keyboard when enter is pressed
        user_input.setOnKeyListener(new View.OnKeyListener()
        {
            /**
             * This listens for the user to press the enter button on
             * the keyboard and then hides the virtual keyboard
             */
            public boolean onKey(View arg0, int arg1, KeyEvent event) {
                // If the event is a key-down event on the "enter" button
                if ( (event.getAction() == KeyEvent.ACTION_DOWN  ) &&
                        (arg1           == KeyEvent.KEYCODE_ENTER)   )
                {
                    InputMethodManager imm = (InputMethodManager)getSystemService(INPUT_METHOD_SERVICE);
                    imm.hideSoftInputFromWindow(user_input.getWindowToken(), 0);
                    return true;
                }
                return false;
            }
        } );
    }

    public void inputEntered() {
        EditText user_input = (EditText) findViewById(R.id.user_input);
        String input = "";
        if (user_input != null) {
            input = user_input.getText().toString();
        }
        ImageView pokemon_imageView = (ImageView) findViewById(R.id.pokemon_imageView);
        if (shadow) {
            fadeFinished = false;
            if (input.equalsIgnoreCase(pokemonArray.get(pokemon_number).get(TAG_NAME))) {
                shadow = false;
                Animation a = new AlphaAnimation(1.00f, 0.00f);

                a.setDuration(1000);
                a.setAnimationListener(new Animation.AnimationListener() {

                    public void onAnimationStart(Animation animation) {
                        // TODO Auto-generated method stub

                    }

                    public void onAnimationRepeat(Animation animation) {
                        // TODO Auto-generated method stub

                    }

                    public void onAnimationEnd(Animation animation) {
                        setPokemonImage();
                        fadeFinished = true;
                    }
                });
                if (pokemon_imageView != null) {
                    pokemon_imageView.startAnimation(a);
                }
            } else {
                lives--;
                setLives();
                if (lives == 0) {
                    Bundle bundle = new Bundle();
                    bundle.putSerializable("array", pokemonArray);
                    bundle.putInt("level",level);
                    bundle.putInt("pokeNo",pokemon_number);
                    bundle.putInt("lives",++lives);
                    bundle.putBoolean("docontinue",true);
                    bundle.putBoolean("adsLeft",adsLeft);
                    Intent i = new Intent(getApplicationContext(), GameOver.class);
                    i.putExtras(bundle);
                    startActivity(i);
                    Toast.makeText(getApplicationContext(), "GameOver !", Toast.LENGTH_LONG).show();
                    preUtil.savePokemonNum("0");
                    finish();
                }
            }
        } else {
            if (pokemon_number < pokemonArray.size() - 1) {
                pokemon_number++;
                shadow = true;
                preUtil.savePokemonNum(String.valueOf(pokemon_number));
            } else {
                Toast.makeText(getApplicationContext(), "No more pokemons", Toast.LENGTH_LONG).show();
                newLevel();
                Intent i = null;

                if(level != 9) {
                    i = new Intent(getApplicationContext(), FirstMenu.class);
                    i.putExtra("later",false);
                }else{
                    i = new Intent(getApplicationContext(),TheEnd.class);
                }
                i.putExtra("adsLeft",adsLeft);
                startActivity(i);
            }
        }

        Log.i("pn", String.valueOf(pokemon_number));
        if (user_input != null) {
            user_input.getText().clear();
        }
        if (fadeFinished) {
            setPokemonImage();
        }
    }

    public void setLives() {
        ImageView life1 = (ImageView) findViewById(R.id.life1);
        ImageView life2 = (ImageView) findViewById(R.id.life2);
        ImageView life3 = (ImageView) findViewById(R.id.life3);
        if (life1 != null && life2 != null && life3 != null) {
            if (lives == 3) {
                life1.setImageResource(R.drawable.life);
                life2.setImageResource(R.drawable.life);
                life3.setImageResource(R.drawable.life);
            } else if (lives == 2) {
                life1.setImageResource(R.drawable.life);
                life2.setImageResource(R.drawable.life);
                life3.setImageResource(R.drawable.life_empty);
            } else if (lives == 1) {
                life1.setImageResource(R.drawable.life);
                life2.setImageResource(R.drawable.life_empty);
                life3.setImageResource(R.drawable.life_empty);
            } else {
                life1.setImageResource(R.drawable.life_empty);
                life2.setImageResource(R.drawable.life_empty);
                life3.setImageResource(R.drawable.life_empty);
            }
        }

        if (lives != 0) {
            preUtil.saveLives(String.valueOf(lives));
        } else {
            preUtil.saveLives("3");
        }
    }

    public void newLevel() {
        if (level == 1) {
            level = 2;
            pokemon_number = 0;

        } else if (level == 2) {
            if (pokemon_number == 20) {
                level = 3;
                pokemon_number = 0;
            }
        } else if (level == 3) {
            if (pokemon_number == 20) {
                level = 5;
                pokemon_number = 0;
            }
        } else if (level == 5) {
            if (pokemon_number == 20) {
                level = 6;
                pokemon_number = 0;
            }
        } else if (level == 6) {
            if (pokemon_number == 20) {
                level = 7;
                pokemon_number = 0;
            }
        } else if (level == 7) {
            if (pokemon_number == 20) {
                level = 9;
                pokemon_number = 0;
            }
        }

        adsLeft = false;
        String levels = String.valueOf(level);
        Log.i("levels", levels);
        preUtil.saveLevel(levels);
        preUtil.savePokemonNum(String.valueOf(pokemon_number));
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();
        Log.i("id",String.valueOf(id));
        if (id == 16908332) {
            Intent i = new Intent(getApplicationContext(),SecondMenu.class);
            Bundle extras = new Bundle();
            extras.putSerializable("array",pokemonArray);
            extras.putInt("position",level);
            extras.putInt("lives",lives);
            extras.putInt("pokeNo",pokemon_number);
            extras.putBoolean("continue",docontinue);
            extras.putBoolean("adsLeft",adsLeft);
            i.putExtras(extras);
            startActivity(i);

        }

        return super.onOptionsItemSelected(item);
    }
}
