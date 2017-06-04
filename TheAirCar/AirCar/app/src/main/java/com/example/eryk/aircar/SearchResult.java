package com.example.eryk.aircar;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;

import com.example.eryk.aircar.listViews.CustomAdapter;
import com.example.eryk.aircar.listViews.CustomAdapter3;
import com.parse.FindCallback;
import com.parse.ParseException;
import com.parse.ParseFile;
import com.parse.ParseObject;
import com.parse.ParseQuery;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.HashMap;
import java.util.List;


/**
 * Created by Eryk on 24/11/16.
 * Class deals with search results
 * Displays results in a listView.
 */

public class SearchResult extends AppCompatActivity {

    // Variables
    private int dayFrom, monthFrom, yearFrom;
    private int dayTo, monthTo, yearTo;
    private int priceFrom, priceTo;
    private String city;
    private String make, model;
    private ListView listView;
    private List<HashMap<String,String>> data; // stores data
    private Calendar from;
    private Calendar to;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_searchresult);

        Bundle bundle = getIntent().getExtras();
        city = bundle.getString("location");

        if(city != null) {
            city = city.toLowerCase();
        }

        dayFrom = bundle.getInt("dayFrom");
        monthFrom = bundle.getInt("monthFrom");
        yearFrom = bundle.getInt("yearFrom");

        dayTo = bundle.getInt("dayTo");
        monthTo = bundle.getInt("monthTo");
        yearTo = bundle.getInt("yearTo");

        priceFrom = bundle.getInt("priceFrom");
        priceTo = bundle.getInt("priceTo");

        make = bundle.getString("make");
        model = bundle.getString("model");

        make = make.toLowerCase();
        model = model.toLowerCase();

        searchForCars(); // look for cars


        listView = (ListView) findViewById(R.id.layout_history_listView);
    }

    private void searchForCars(){
        // Convert the integers to dates
        from = Calendar.getInstance();
        to = Calendar.getInstance();
        from.set(yearFrom,monthFrom,dayFrom);
        to.set(yearTo,monthTo,dayTo);

        ParseQuery<ParseObject> findCars = new ParseQuery<>(MainActivity.TAGS.CARPOST_TAG); // Make a query
        findCars.whereLessThanOrEqualTo(MainActivity.TAGS.AFROM_TAG,from.getTime());  // where clause (from dates)
        findCars.whereGreaterThanOrEqualTo(MainActivity.TAGS.ATO_TAG,to.getTime());   // where clause (to dates)
        findCars.whereEqualTo(MainActivity.TAGS.CITY_TAG,city);                       // city clause (location)
        findCars.whereLessThanOrEqualTo(MainActivity.TAGS.PRICE_TAG,priceTo);         // check the price range (max)
        findCars.whereGreaterThanOrEqualTo(MainActivity.TAGS.PRICE_TAG,priceFrom);    // price range (minimum)

        //region LOGS
        Log.i("AFROM: ",from.getTime().toString());
        Log.i("ATO: ", to.getTime().toString());
        Log.i("City: ", city);
        Log.i("PriceFrom: ", String.valueOf(priceFrom));
        Log.i("PriceTo: ", String.valueOf(priceTo));
        //endregion

        // Check if user specified make and model of the car, if so add constraints to the query
        if(!make.equals(MainActivity.TAGS.ANYMAKE_TAG)){
            Log.i("Search: ","make if");
            findCars.whereEqualTo(MainActivity.TAGS.MAKE_TAG,make);
        }

        if(!model.equals(MainActivity.TAGS.ANYMODEL_TAG)){
            Log.i("Search: ","model if");
            findCars.whereEqualTo(MainActivity.TAGS.MODEL_TAG,model);
        }

        findCars.findInBackground(new FindCallback<ParseObject>() { // Execute query
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // Done executing
                Log.i("SearchResult","Done");
                if(e == null){ // no errors
                    if(objects.size() > 0){ // more than 0 objects found


                        data = new ArrayList<HashMap<String, String>>(); // initialize data

                        int j = 0; // j used to avoid null pointer exception
                        for (int i = 0; i < objects.size(); i++) {
                            HashMap<String, String> localHashMap = new HashMap<String, String>();
                            if (objects.get(i).getInt(MainActivity.TAGS.POSTED_TAG) == (MainActivity.TAGS.ONE_TAG)) {
                                // Get image as ParseFile
                                ParseFile postImage = objects.get(i).getParseFile(MainActivity.TAGS.CAR_PICTURE);
                                String imageUrl = postImage.getUrl(); // Get the image URL

                                localHashMap.put(MainActivity.TAGS.DESCRIPTION_TAG, objects.get(i).get(MainActivity.TAGS.DESCRIPTION_TAG).toString());
                                localHashMap.put(MainActivity.TAGS.OBJECTID_TAG, objects.get(i).getObjectId());
                                localHashMap.put(MainActivity.TAGS.URL_TAG,imageUrl); // Store it to the temp HashMap
                                data.add(j, localHashMap);
                                j = i;
                            }else{
                                Log.i("SearchResult: ","Not posted");
                                if(j > 0) {
                                    j = i - 1;
                                }
                            }
                        }
                        setAdapter();
                    }else{ // posts not found
                        Log.i("SearchResult","Fail1");
                        noDataFound();
                    }
                }else{ // error occurred
                    e.printStackTrace();
                    Log.i("SearchResult","Fail2");
                    noDataFound();
                }
            }
        });
    }

    private void noDataFound(){
        // set no data found (as an element of listview)
        String NO_DATA_FOUND = getResources().getString(R.string.noCarsFound);
        String[] countries = new String[] {NO_DATA_FOUND};
        final ArrayList<String> list  = new ArrayList<>();
        for(int i =0; i < countries.length; i++){
            list.add(i,countries[i]);
        }
        CustomAdapter adapter = new CustomAdapter(this,R.layout.advanced_list_row,list);
        listView.setAdapter(adapter);
    }

    private void setAdapter(){
        // set adapater
        CustomAdapter3 adapter = new CustomAdapter3(this,data);
        listView.setAdapter(adapter);
        // set on click listener
        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                Intent i = new Intent(getApplicationContext(),Car.class);
                i.putExtra(MainActivity.TAGS.POSTID_TAG,data.get(position).get(MainActivity.TAGS.OBJECTID_TAG));
                i.putExtra(MainActivity.TAGS.RENTFROM_TAG,(Serializable) from);
                i.putExtra(MainActivity.TAGS.RENTTO_TAG, (Serializable) to);
                i.putExtra(MainActivity.TAGS.SHOW_RESERVE_TAG,true);
                startActivity(i); // start car class
            }
        });
    }

    @Override
    public void onBackPressed() {
        Intent i = new Intent(getApplicationContext(), MainActivity.class); // main class
        startActivity(i);
    }
}
