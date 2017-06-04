package com.example.eryk.aircar;

import android.content.Intent;
import android.net.Uri;
import android.os.Handler;
import android.os.Message;
import android.support.v4.view.ViewPager;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.util.Patterns;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.parse.LogInCallback;
import com.parse.ParseException;
import com.parse.ParseUser;

import java.util.Calendar;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

/**
 * Created by Eryk on 19/11/16.
 */
public class LoginActivity extends AppCompatActivity {

    EditText username, password;            // Variables for username and password edittext
    private static long theTime = 0;        // Stores time which is constantly compared with current time in background thread (used for animation)
    private ViewPager viewPager;            // ViewPager that displays 'the images'
    private boolean runThread;              // A boolean to control the background thread. (Improves termination speed).
    ExecutorService threadPoolExecutor;     // Executor for the thread (In order to terminate thread i.e. viePager functionality, when user already logged in)
    Future longRunningTaskFuture;           // Future is used to 'record' the Runnable to poolExecutor, and to terminate using .cancel();

    // Handler is used to update the UI element on main thread i.e. the viewPager
    // When received msg the handler will execute, this will animate the viewPager
    private Handler messageHandler = new Handler() {
        public void handleMessage(Message msg) {
            super.handleMessage(msg);
            int item = viewPager.getCurrentItem();   // Get current item of the viePager (0,1 or 2);
            if (item < 2) {                          // If item is less than 2 then increment it
                item++;
            } else {
                item = 0;                            // If it is not then reset it.
            }
            viewPager.setCurrentItem(item);          // set the current item of viewPager
        }
    };

    /*
        OnCreate method is called at the start of activity.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);  // set content view to the appropriate layout

        threadPoolExecutor = Executors.newSingleThreadExecutor();  // Initialize new executor (singleThread).

        if(ParseUser.getCurrentUser() != null) {  // Check if user is logged in, if so there move to the main activity.
            runThread = false;  // This will stop thread if running
            if(longRunningTaskFuture != null) {
                longRunningTaskFuture.cancel(true); // This will terminate thread if running.
                threadPoolExecutor.shutdown();      // Shutdown threadExecutor
            }
            // Start main activity.
            Intent i = new Intent(getApplicationContext(),MainActivity.class);
            startActivity(i);
        }else { // That means the user is not logged in

            viewPager = (ViewPager) findViewById(R.id.activity_login_image);        // Find ViewPager
            CustomSwipeAdapter customSwipeAdapter = new CustomSwipeAdapter(this);   // Apply CustomSwipeAdapter
            viewPager.setAdapter(customSwipeAdapter);                               // Set the adapter of viewPager

            // Find edit texts
            username = (EditText) findViewById(R.id.activity_login_email);
            password = (EditText) findViewById(R.id.activity_login_password);


            // Find login button
            Button loginButton = (Button) findViewById(R.id.activity_login_loginButton);
            // Set onClickListener to the button
            loginButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    login(); // Call login function when the 'loginButton' has been clicked.
                }
            });

            // Find registerTextView (which is used as a button to start the Register Activity).
            TextView registerButton = (TextView) findViewById(R.id.activity_login_registerButton);
            // Set onClickListener to the TextView (button).
            registerButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    runThread = false;  // If there was no exception, stop whileloop inside the 'runnable'
                    if(longRunningTaskFuture != null) {
                        longRunningTaskFuture.cancel(true); // Terminate the thread (which updates viewPager)
                        threadPoolExecutor.shutdown();      // Shutdown threadExecutor
                    }
                    // Start RegisterActivity
                    Intent i = new Intent(getApplicationContext(), RegisterActivity.class);
                    startActivity(i);
                }
            });

            // Find postCarTextView (which is used as a button to start google play store)
            TextView postCarButton = (TextView) findViewById(R.id.activity_login_postCarButton);
            // Set on click listener
            postCarButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    /*
                           * ******** PLEASE NOTE, THIS WILL START GOOGLE PLAY STORE, ON APP NAMED: friend slice (my other app) ************ *
                           * ******** IN PRODUCTION, THIS WOULD REDIRECT USER TO THE BUSINESS APP (POST A CAR APP)              ************ *
                           * ******** THIS IS JUST USED TO SHOW MY ABILITY TO START GOOGLE PLAY STORE                           ************ *
                     */
                    startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("https://play.google.com/store/apps/details?id=erpam.friend_slice")));
                }
            });


            runThread = true;   // Set boolean to true, so the 'runnable' can run. (the while loop boolean).
            setThread();        // Call the function setThread();
        }
    }

    /*
        This method is called from onCreate().
        It is responsible for executing the 'runnable' in the background.
     */
    private void setThread(){
        Calendar calendar = Calendar.getInstance();         // Get calendar instance
        theTime = calendar.getTimeInMillis();               // Set theTime to current time
        Runnable runnable = new Runnable() {                // Initialize new Runnable
            @Override
            public void run() {                                 // Overide run method
                while (runThread) {                             // Keep looping until runThread is changed (will change on user login)
                    Calendar calendar = Calendar.getInstance(); // Get another calendar isntance.
                    long time = calendar.getTimeInMillis();     // Get the CURRENT time
                    if ((time - theTime) > 3000) {              // Compare CURRENT time with 'theTime' (which stores the previous time). If > 3000 that means 3 seconds have passed.
                        messageHandler.sendEmptyMessage(0);     // (After 3 seconds) send a message to the handler to update the UI (ViewPager)
                        theTime = time;                         // Update theTime to CURRENT time.
                    }
                }
            }
        };

        longRunningTaskFuture = threadPoolExecutor.submit(runnable); // Start runnable
    }


    /*
        Login function is responsible for validating and login the user.
        It is called twice in this activity:
            - At the beginning of onCreate() method.
            - When login button is pressed.
     */
    private void login(){
        // First check if the username and password are not blank
        if(!username.getText().toString().equals("") && !password.getText().toString().equals("")){
            if(checkEmail(username.getText().toString())) { // Validate email format
                if ((password.getText().toString().length() > MainActivity.TAGS.SIX_TAG)) {  // Check the length of the password
                    // Login user in the background
                    ParseUser.logInInBackground(username.getText().toString(), password.getText().toString(), new LogInCallback() {
                        // Overide LogInCallback methods.
                        @Override
                        public void done(ParseUser user, ParseException e) {
                            if (e == null) { // Check if there was any exception
                                Log.i("LOGIN", "SUCCESS");
                                runThread = false;  // If there was no exception, stop whileloop inside the 'runnable'
                                longRunningTaskFuture.cancel(true); // Terminate the thread (which updates viewPager)
                                threadPoolExecutor.shutdown();      // Shutdown threadExecutor
                                // Start MainActivity
                                Intent i = new Intent(getApplicationContext(), MainActivity.class);
                                startActivity(i);
                            } else {
                                e.printStackTrace(); // Print stack trace to find out the error that occured
                                // If user login was unsuccessful then clear the username and password fields
                                username.setText("");
                                password.setText("");
                                // Display appropriate msg to inform the user that login have failed
                                Toast.makeText(LoginActivity.this, R.string.loginFailed, Toast.LENGTH_SHORT).show();
                            }
                        }
                    });
                } else {
                    Toast.makeText(this, R.string.shortCridentials, Toast.LENGTH_SHORT).show(); // Display error msg (short password)
                }
            }else{
                Toast.makeText(this, R.string.invalidEmail, Toast.LENGTH_SHORT).show(); // Display error msg (invalid email format)
            }
        }else{
            Toast.makeText(this, R.string.blankCridentials, Toast.LENGTH_SHORT).show(); // Display error msg (blank credentials)
        }
    }

    /*
        Method to validate email format
        Takes one parameter which is the email string.
     */
    private boolean checkEmail(String email){

        if(Patterns.EMAIL_ADDRESS.matcher(email).matches()) // Check if email matches the email pattern
        {
            return true; // if so return true
        }else{
            return false; // otherwise return false
        }
    }
}
