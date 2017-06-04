package com.example.eryk.aircar;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Patterns;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import com.parse.LogInCallback;
import com.parse.ParseException;
import com.parse.ParseUser;
import com.parse.SaveCallback;
/**
 * Created by Eryk on 25/11/16.
 * Deals with updating details.
 */

public class UpdateDetails extends AppCompatActivity {

    // Variables
    private EditText email,pass,repass;
    private Button button;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_update_details);

        setObjects();
    }


    private void setObjects(){
        // Find elements of the layout
        email = (EditText) findViewById(R.id.update_email);
        pass = (EditText) findViewById(R.id.update_password);
        repass = (EditText) findViewById(R.id.update_repass);
        button = (Button) findViewById(R.id.update_button);
        // set button on click listener
        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                    if(checkFormats()) { // check formats
                        update(); // update
                    }
            }
        });

    }

    private boolean checkFormats(){
        // Method to validate formats

        // Check email format
        if(!(Patterns.EMAIL_ADDRESS.matcher(email.getText()).matches()))
        {
            Toast.makeText(this, R.string.invalidEmail, Toast.LENGTH_SHORT).show();
            return false;
        }
        // Check if anything is left blank
        if(email.getText().toString().equals("") || pass.getText().toString().equals("") || repass.getText().toString().equals("")) {
            Toast.makeText(this, R.string.blankCridentials, Toast.LENGTH_SHORT).show();
            return false;
        }
        // Check password length
        if(pass.length() < MainActivity.TAGS.SIX_TAG || repass.length() < MainActivity.TAGS.SIX_TAG){
            Toast.makeText(this, R.string.shortCridentials, Toast.LENGTH_SHORT).show();
            return false;
        }
        // Check password match
        if(!pass.getText().toString().equals(repass.getText().toString())){
            Toast.makeText(this, R.string.passwordDontMatch, Toast.LENGTH_SHORT).show();
            return false;
        }

        return true; // means everything is okay
    }



    private void update() {
        ParseUser user = ParseUser.getCurrentUser(); // current user
        user.setUsername(email.getText().toString()); // set email
        user.setPassword(pass.getText().toString()); // set password
        user.saveInBackground(new SaveCallback() { // save
            @Override
            public void done(ParseException e) {
                if(e == null){ // no exceptions
                    Toast.makeText(UpdateDetails.this, R.string.updateSuccess, Toast.LENGTH_SHORT).show(); // inform user
                    ParseUser.logOut(); // log out
                    ParseUser.logInInBackground(email.getText().toString(), pass.getText().toString(), new LogInCallback() {
                        @Override
                        public void done(ParseUser user, ParseException e) { // login
                            if(e != null){
                                Toast.makeText(UpdateDetails.this, R.string.somethingWrong, Toast.LENGTH_SHORT).show(); // instruct user
                                Intent i = new Intent(getApplicationContext(),LoginActivity.class);
                                startActivity(i); // start login class
                            }
                        }
                    });

                }else{
                    Toast.makeText(UpdateDetails.this, R.string.updateFailed, Toast.LENGTH_SHORT).show(); // inform user
                }
            }
        });
    }

    @Override
    public void onBackPressed() {
        Intent i = new Intent(getApplicationContext(),MainActivity.class);
        startActivity(i);
    }
}
