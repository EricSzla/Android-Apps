package com.example.eryk.aircar;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.support.v4.view.ViewPager;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.parse.FindCallback;
import com.parse.GetCallback;
import com.parse.Parse;
import com.parse.ParseException;
import com.parse.ParseFile;
import com.parse.ParseObject;
import com.parse.ParseQuery;
import com.parse.ParseUser;
import com.parse.SaveCallback;

import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;

/**
 * Created by Eryk on 25/11/16.
 * Class to deal with Car activity.
 */

public class Car extends AppCompatActivity {

    private HashMap<String,String> carInfoHashMap;  // Variable which will store all the car data
    private Calendar rentFrom; // from calendar
    private Calendar rentTo; // to calendar
    private boolean hideButtons; // boolean (which is received from the bundle), determines if Reserve and Save button should be visible
    private boolean changeBack;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.progress_bar);
        final Bundle bundle = getIntent().getExtras(); // get extras
        String id = "";
        changeBack = false;
        if(bundle != null) {
            id = bundle.getString(MainActivity.TAGS.POSTID_TAG);
            rentFrom = (Calendar) bundle.get(MainActivity.TAGS.RENTFROM_TAG);
            rentTo = (Calendar) bundle.get(MainActivity.TAGS.RENTTO_TAG);
            hideButtons = (boolean) bundle.get(MainActivity.TAGS.SHOW_RESERVE_TAG);
            if(bundle.get(MainActivity.TAGS.BACKCHANGED_TAG) != null) {
                changeBack = (boolean) bundle.get(MainActivity.TAGS.BACKCHANGED_TAG);
            }
            findPost(id); // start looking for posts

        }else{
            Intent i = new Intent(getApplicationContext(),MainActivity.class);
            startActivity(i);
        }
    }

    private void findPost(final String fav_id){
        ParseQuery<ParseObject> posts_query = new ParseQuery<>(MainActivity.TAGS.CAR_POST); // query carPost collection on mongoDB
        posts_query.whereEqualTo(MainActivity.TAGS.OBJECTID_TAG,fav_id);    // where objectid = fav_id (from parameter)
        posts_query.setLimit(1); // find only one
        posts_query.findInBackground(new FindCallback<ParseObject>() {
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // find in background
                // Data has been found
                if(e == null && objects.size() > 0) {
                    if (carInfoHashMap == null) {  // Initialize HashMap
                        carInfoHashMap = new HashMap<>();
                    }

                    // Retrieve dates
                    Date from = objects.get(MainActivity.TAGS.ZERO_TAG).getDate(MainActivity.TAGS.AFROM_TAG);
                    Calendar fromCalendar = Calendar.getInstance();
                    fromCalendar.setTime(from);

                    Date to = objects.get(MainActivity.TAGS.ZERO_TAG).getDate(MainActivity.TAGS.ATO_TAG);
                    Calendar toCalendar = Calendar.getInstance();
                    toCalendar.setTime(to);

                    String month = fromCalendar.getDisplayName(Calendar.MONTH, Calendar.SHORT, Locale.getDefault());
                    int year  = fromCalendar.get(Calendar.YEAR);
                    int day = fromCalendar.get(Calendar.DAY_OF_MONTH);
                    String fromDate = String.valueOf(day) + "/" + month + "/" + String.valueOf(year);

                    month = toCalendar.getDisplayName(Calendar.MONTH, Calendar.SHORT, Locale.getDefault());
                    year  = toCalendar.get(Calendar.YEAR);
                    day = toCalendar.get(Calendar.DAY_OF_MONTH);
                    String toDate = String.valueOf(day) + "/" + month + "/" + String.valueOf(year);


                    // Store data in the HashMap
                    carInfoHashMap.put(MainActivity.TAGS.POSTID_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).getObjectId());
                    carInfoHashMap.put(MainActivity.TAGS.DESCRIPTION, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.DESCRIPTION).toString());
                    carInfoHashMap.put(MainActivity.TAGS.USERID_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.USERID_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.REG_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.REG_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.MAKE_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.MAKE_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.MODEL_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.MODEL_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.ENGINE_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.ENGINE_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.AFROM_TAG, fromDate);
                    carInfoHashMap.put(MainActivity.TAGS.ATO_TAG, toDate);
                    carInfoHashMap.put(MainActivity.TAGS.CITY_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.CITY_TAG).toString());
                    carInfoHashMap.put(MainActivity.TAGS.PRICE_TAG, objects.get(MainActivity.TAGS.ZERO_TAG).get(MainActivity.TAGS.PRICE_TAG).toString());

                    // Variables for images
                    Uri uri;
                    // Store images in the HashMap
                    for (int i = 0; i < 3; i++) {
                        ParseFile postImage = objects.get(0).getParseFile((MainActivity.TAGS.CAR_PICTURE0 + (i + 1))); // get image
                        String imageUrl = postImage.getUrl(); // Get image url
                        uri = Uri.parse(imageUrl);
                        carInfoHashMap.put(MainActivity.TAGS.URL_TAG + i, uri.toString()); // Store it to the temp HashMap
                    }

                    setContent(); // Set the layout

                } else {
                    Toast.makeText(Car.this, R.string.carFetchFail, Toast.LENGTH_SHORT).show();
                    onBackPressed();
                }

            }
        });
    }

    @Override
    public void onBackPressed() {
        if(!hideButtons || changeBack){
            Intent i = new Intent(getApplicationContext(),MainActivity.class);
            startActivity(i);
        }else{
            super.onBackPressed();
        }
    }

    private void setContent(){
        setContentView(R.layout.activity_car);

        ViewPager viewPager = (ViewPager) findViewById(R.id.activity_car_viewPager);
        Uri[] uris = new Uri[3];
        for(int i=0; i < 3; i++){
            uris[i] = Uri.parse(carInfoHashMap.get(MainActivity.TAGS.URL_TAG + i));
        }
        CustomSwipeAdapter2 customSwipeAdapter = new CustomSwipeAdapter2(this,uris);
        viewPager.setAdapter(customSwipeAdapter);

        // Initialize text Views and button.
        Button reserveButton = (Button) findViewById(R.id.activity_car_reserveButton);
        Button saveButton = (Button) findViewById(R.id.activity_car_addToFavButton);
        TextView make = (TextView) findViewById(R.id.activity_car_makeTextView);
        TextView model = (TextView) findViewById(R.id.activity_car_modelTextView);
        TextView aFrom = (TextView) findViewById(R.id.activity_car_availableFrom);
        TextView aTo = (TextView) findViewById(R.id.activity_car_availableTo);
        TextView desc = (TextView) findViewById(R.id.activity_car_descriptionTextView);
        TextView engine = (TextView) findViewById(R.id.activity_car_engineTextView);
        TextView price = (TextView) findViewById(R.id.activity_car_priceDayTextView);

        // Set listener to the button.
        reserveButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                setContentView(R.layout.progress_bar);
                reserveCar();
            }
        });

        saveButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                setContentView(R.layout.progress_bar);
                checkSaveCar();
            }
        });


        if(!hideButtons){
            reserveButton.setVisibility(View.GONE);
            saveButton.setVisibility(View.GONE);
        }


        String pricewithEuro = getString(R.string.euro) + carInfoHashMap.get(MainActivity.TAGS.PRICE_TAG);
        // Populate text views with actual data.
        make.setText(carInfoHashMap.get(MainActivity.TAGS.MAKE_TAG));
        model.setText(carInfoHashMap.get(MainActivity.TAGS.MODEL_TAG));
        aFrom.setText(carInfoHashMap.get(MainActivity.TAGS.AFROM_TAG));
        aTo.setText(carInfoHashMap.get(MainActivity.TAGS.ATO_TAG));
        desc.setText(carInfoHashMap.get(MainActivity.TAGS.DESCRIPTION));
        engine.setText(carInfoHashMap.get(MainActivity.TAGS.ENGINE_TAG));
        price.setText(pricewithEuro);
    }

    private void checkSaveCar(){
        // Check if the car was previously saved
        ParseQuery<ParseObject> favQuery = ParseQuery.getQuery(MainActivity.TAGS.FAVOURITE);  // getQuery collection FAVOURITE
        favQuery.whereEqualTo(MainActivity.TAGS.POSTID_TAG,carInfoHashMap.get(MainActivity.TAGS.POSTID_TAG)); // where postid = carInfo.get(postid)
        favQuery.whereEqualTo(MainActivity.TAGS.USERID_TAG,ParseUser.getCurrentUser().getObjectId());   // where userid = current user id
        favQuery.getFirstInBackground(new GetCallback<ParseObject>() {
            @Override
            public void done(ParseObject object, ParseException e) { // get first in background (means stop iterating through data once any object matching the criteria is found)
                if(e == null){
                    Toast.makeText(Car.this, R.string.carSaved, Toast.LENGTH_SHORT).show(); // inform user
                    setContent(); // set back the content
                }else{
                    saveCar(); // start saving car
                }
            }
        });
    }

    private void saveCar(){
        ParseObject favObject = new ParseObject(MainActivity.TAGS.FAVOURITE);                   // new favourite object
        favObject.put(MainActivity.TAGS.USERID_TAG,ParseUser.getCurrentUser().getObjectId());   // put userid
        favObject.put(MainActivity.TAGS.POSTID_TAG,carInfoHashMap.get(MainActivity.TAGS.POSTID_TAG)); // put postid
        favObject.saveInBackground(new SaveCallback() {
            @Override
            public void done(ParseException e) { // save
                if(e == null){
                    setContent(); // set content
                    Toast.makeText(Car.this, R.string.carSaveSuccess, Toast.LENGTH_SHORT).show(); // inform user
                }else{
                    setContent(); // set content
                    Toast.makeText(Car.this, R.string.carSaveNotSuccess, Toast.LENGTH_SHORT).show(); // inform user that failed
                }
            }
        });
    }

    private void reserveCar(){
        // Reserve a car method
        ParseQuery<ParseObject> query = ParseQuery.getQuery(MainActivity.TAGS.NOTIFICATIONS_TAG);               // get query from notifications collection
        query.whereEqualTo(MainActivity.TAGS.POSTERID_TAG,carInfoHashMap.get(MainActivity.TAGS.USERID_TAG));    // where posterid = carinfo.get(userid)
        query.whereEqualTo(MainActivity.TAGS.CUSTOMERID_TAG,ParseUser.getCurrentUser().getObjectId());          // where customerid = current user id
        query.whereEqualTo(MainActivity.TAGS.POSTID_TAG,carInfoHashMap.get(MainActivity.TAGS.POSTID_TAG));      // where postid = carinfo post id
        query.getFirstInBackground(new GetCallback<ParseObject>() {
            @Override
            public void done(ParseObject object, ParseException e) {
                if(e == null){
                    // means post have been found, so user has reserved this car already
                    Toast.makeText(Car.this, R.string.alreadyReserved, Toast.LENGTH_SHORT).show(); // inform user
                    setContent(); // set content
                }else{
                    ParseObject historyObject = new ParseObject(MainActivity.TAGS.HISTORY_TAG);         // new HISTORY collection object
                    historyObject.put(MainActivity.TAGS.POSTID_TAG,carInfoHashMap.get(MainActivity.TAGS.POSTID_TAG)); // put post id
                    historyObject.put(MainActivity.TAGS.USERID_TAG, ParseUser.getCurrentUser().getObjectId()); // put userid
                    historyObject.put(MainActivity.TAGS.RENTFROM_TAG,rentFrom.getTime()); // put rentfrom
                    historyObject.put(MainActivity.TAGS.RENTTO_TAG,rentTo.getTime()); // put rent to
                    // save object
                    historyObject.saveInBackground(new SaveCallback() {
                        @Override
                        public void done(ParseException e) {
                            if(e == null){
                                updateMsg(); // update notifications
                                Log.i("CAR: ", "GETTING POSTER EMAIL");
                            }else{
                                setContent(); // set content
                                Toast.makeText(Car.this, R.string.carNotReserved, Toast.LENGTH_SHORT).show(); // inform user
                            }
                        }
                    });
                }
            }
        });


    }

    private void updateMsg(){
        // Calculate for how many days the user wants to rent a car.
        long differenceInMillis = rentTo.getTimeInMillis() - rentFrom.getTimeInMillis();
        long days = differenceInMillis / (24 * 60 * 60 * 1000);
        String dayString = " days";
        if (days == 0 || days == 1){
            days = 1;
            dayString = " day";
        }
        // construct msg
        final String msg = getResources().getString(R.string.hello) +  days + dayString + getResources().getString(R.string.inDates) + "\r\n"
                + rentFrom.getTime() + " - " + rentTo.getTime();

        ParseObject parsequery = new ParseObject(MainActivity.TAGS.NOTIFICATIONS_TAG);   // new NOTIFICATION collection object
        parsequery.put(MainActivity.TAGS.POSTERID_TAG,carInfoHashMap.get(MainActivity.TAGS.USERID_TAG)); // put posterid
        parsequery.put(MainActivity.TAGS.CUSTOMERID_TAG,ParseUser.getCurrentUser().getObjectId()); // put customer id
        parsequery.put(MainActivity.TAGS.POSTID_TAG,carInfoHashMap.get(MainActivity.TAGS.POSTID_TAG)); // put postid
        parsequery.put(MainActivity.TAGS.MESSAGE_TAG,msg); // put msg
        parsequery.put(MainActivity.TAGS.EMAIL_TAG, ParseUser.getCurrentUser().getEmail());
        parsequery.saveInBackground(new SaveCallback() {
            @Override
            public void done(ParseException e) {
                if(e == null){ // success
                    Toast.makeText(Car.this, R.string.carReserved, Toast.LENGTH_SHORT).show(); // inform user
                    setContent(); // set content
                }else{
                    Toast.makeText(Car.this, R.string.carNotReserved, Toast.LENGTH_SHORT).show(); // fail, inform user
                    setContent(); // setcontent
                }
            }
        });
    }
}
