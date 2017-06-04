package com.example.eryk.aircar;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.util.Patterns;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.Toast;

import com.parse.LogInCallback;
import com.parse.ParseException;
import com.parse.ParseUser;
import com.parse.SignUpCallback;

/**
 * Created by Eryk on 19/11/16.
 */

public class RegisterActivity extends AppCompatActivity {

    EditText email, password, repass;  // Variables for username, password and repass edittext
    RadioGroup experienceGroup;        // Variable for radioGroup

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        initialize(); // Call initialize method.
    }

    private void initialize(){
        Button registerButton = (Button) findViewById(R.id.activity_register_regButton);  // Find register Button
        email = (EditText) findViewById(R.id.activity_register_email);                    // Find email editText
        password = (EditText) findViewById(R.id.activity_register_password);              // Find password editText
        repass = (EditText) findViewById(R.id.activity_register_repassword);              // Find repass editText
        experienceGroup = (RadioGroup) findViewById(R.id.activity_register_radioGroup);   // Find radiogroup

        // Set onClickListener to registerButton
        registerButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                int radioButtonID = experienceGroup.getCheckedRadioButtonId();          // Get the radioButtonId
                Log.i("RadioButtonId: ", String.valueOf(radioButtonID));
                RadioButton radioButton = (RadioButton) findViewById(radioButtonID);    // find radioButton (using radioButtonId)
                int id = experienceGroup.indexOfChild(radioButton);                     // get radioGroup index
                Log.i("Index: ", String.valueOf(id));

                // Receive the strings
                String emailS,passS,repassS;
                emailS = email.getText().toString();        // Get text for email
                passS = password.getText().toString();      // Get text for pass
                repassS = repass.getText().toString();      // Get text for repass

                if (checkFormats(emailS, passS, repassS)){ // validate input
                    if(radioButton == null){ // Check if experience was selected
                        Toast.makeText(RegisterActivity.this, R.string.selectExperience, Toast.LENGTH_SHORT).show();  // Display Error msg (select exp)
                    }else {
                        int exp = 0; // Local variable for experience
                        switch (id) { // Position of radiogroup selected
                            // Set experience depending on the id
                            case 1:
                                exp = 0;
                                break;
                            case 2:
                                exp = 5;
                                break;
                            case 3:
                                exp = 10;
                                break;
                            case 4:
                                exp = 15;
                                break;
                        }
                        setContentView(R.layout.progress_bar); // Change the layout to progress bar
                        register(exp,emailS,passS); // Call register method and pass experience,email,pass as parameters
                    }
                }

            }
        });
    }

    /*
        This method is used to register user.
     */
    private void register(int exp,final String emailText,final String passwordText){
        ParseUser user = new ParseUser();                               // Create new user
        user.setEmail(emailText);                                       // Set user's email
        user.setUsername(emailText);                                    // Set user's username
        user.setPassword(passwordText);                                 // SEt user's password
        user.put(MainActivity.TAGS.EXPERIENCE_TAG,String.valueOf(exp)); // Set user's experience
        // SignUp user in the background.
        user.signUpInBackground(new SignUpCallback() { // Sign up the user
            @Override
            public void done(ParseException e) {
                if(e == null){ // If there is no errors
                    // Inform the user
                    Toast.makeText(RegisterActivity.this, R.string.registrationSuccess, Toast.LENGTH_SHORT).show();
                    login(emailText, passwordText); // login
                }else{
                    setContentView(R.layout.activity_register); // set content back to register and display error msg
                    Toast.makeText(RegisterActivity.this, R.string.registrationFailed, Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    // Login method, logs in the user.
    private void login(String username, String pass){
        ParseUser.logInInBackground(username,pass, new LogInCallback() {
            @Override
            public void done(ParseUser user, ParseException e) {
                if (e == null) { // If there is no errors
                    // Inform the user and start Main Activity
                    Toast.makeText(RegisterActivity.this, R.string.loginSuccess, Toast.LENGTH_SHORT).show();
                    Intent i = new Intent(getApplicationContext(), MainActivity.class);
                    startActivity(i);
                } else {
                    // Set content view back to register, reset the field and inform the user that login failed.
                    setContentView(R.layout.activity_register);
                    email.setText("");
                    password.setText("");
                    Toast.makeText(RegisterActivity.this, R.string.loginFailed, Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    // Method used to validate input
    private boolean checkFormats(String email, String pass, String repass){

        // If email format doesn't match the pattern then display msg and return false.
        if(!(Patterns.EMAIL_ADDRESS.matcher(email).matches()))
        {
            Toast.makeText(this, R.string.invalidEmail, Toast.LENGTH_SHORT).show();
            return false;
        }
        // If any of user's input is empty, display msg and return false.
        if(email.equals("") || pass.equals("") || repass.equals("")) {
            Toast.makeText(this, R.string.blankCridentials, Toast.LENGTH_SHORT).show();
            return false;
        }
        // If password length is less than 6 then display msg and return false.
        if(pass.length() < MainActivity.TAGS.SIX_TAG || repass.length() < MainActivity.TAGS.SIX_TAG){
            Toast.makeText(this, R.string.shortCridentials, Toast.LENGTH_SHORT).show();
            return false;
        }
        // If passwords dont match, display msg and return false.
        if(!pass.equals(repass)){
            Toast.makeText(this, R.string.passwordDontMatch, Toast.LENGTH_SHORT).show();
            return false;
        }

        // Return true if validation is success.
        return true;
    }

    // Overide onBackPressed
    @Override
    public void onBackPressed() {
        // Overide in order to start activity (in order for the ViewPager thread to be started again)
        Intent i = new Intent(getApplicationContext(),LoginActivity.class);
        startActivity(i);
    }
}
