package erpam.xo;

import android.provider.Settings;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.Layout;
import android.view.View;
import android.widget.GridLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

public class MainActivity extends AppCompatActivity {
    // 0 = O, 1 = X
    int activePlayer = 0;
    // 2 = unplayed
    int [] gameState = {2,2,2,2,2,2,2,2,2};
    int [][] winningPositions = {{0,1,2},{3,4,5},{6,7,8},{0,3,6},{1,4,7},{2,5,8},{0,4,8},{2,4,6}};
    boolean gameActive = true;

    public void dropIn(View view) {

        ImageView counter = (ImageView) view;
        int tappedCounter = Integer.parseInt(counter.getTag().toString());

        if(gameState[tappedCounter] == 2 && gameActive == true) {
            gameState[tappedCounter] = activePlayer;


            if (activePlayer == 0) {
                counter.setImageResource(R.drawable.o);
                activePlayer = 1;
            } else if (activePlayer == 1) {
                counter.setImageResource(R.drawable.x);
                activePlayer = 0;
            }
            counter.setTranslationY(-100f);
            counter.animate().translationYBy(100f).rotation(360).setDuration(300);

            for(int[] winningPosition : winningPositions)
            {
                if(gameState[winningPosition[0]] == gameState[winningPosition[1]] &&
                        gameState[winningPosition[1]] == gameState[winningPosition[2]] &&
                        gameState[winningPosition[0]] != 2){
                    gameActive = false;
                    String var = "";
                    if(gameState[winningPosition[0]] == 0)
                    {
                        var = "O";
                    }else
                    {
                        var = "X";
                    }

                    TextView textMsg = (TextView) findViewById(R.id.playAgainMsg);
                    textMsg.setText(var + " has won!");
                    LinearLayout layout = (LinearLayout) findViewById(R.id.playAgainLayout);
                    layout.setVisibility(View.VISIBLE);
                    layout.setTranslationY(-100f);
                    layout.animate().translationYBy(100f).rotation(360).setDuration(300);

                }else
                {
                    boolean gameOver = true;
                    for(int counterState : gameState){
                        if(counterState == 2)
                        {
                            gameOver = false;
                        }
                    }

                    if(gameOver)
                    {
                        TextView textMsg = (TextView) findViewById(R.id.playAgainMsg);
                        textMsg.setText("Its a draw!");
                        LinearLayout layout = (LinearLayout) findViewById(R.id.playAgainLayout);
                        layout.setVisibility(View.VISIBLE);
                        layout.setTranslationY(-100f);
                        layout.animate().translationYBy(100f).rotation(360).setDuration(300);
                    }
                }

            }
        }
    }

    public void playAgain(View view){
        LinearLayout layout = (LinearLayout) findViewById(R.id.playAgainLayout);
        layout.setVisibility(View.INVISIBLE);

        // 0 = O, 1 = X
        activePlayer = 0;
        // 2 = unplayed
        for(int i =0; i < gameState.length;i++){
            gameState[i] = 2;
        }

        GridLayout imageLayout = (GridLayout)findViewById(R.id.imageLayout);
        for(int i =0; i < imageLayout.getChildCount();i++) {
            ((ImageView) imageLayout.getChildAt(i)).setImageResource(0);
        }
        gameActive = true;
    }


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
    }
}
