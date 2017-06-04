package com.example.eryk.aircar.fragments;

import android.annotation.SuppressLint;
import android.app.DatePickerDialog;
import android.app.Dialog;
import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.PorterDuff;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.support.design.widget.FloatingActionButton;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.Fragment;

import android.support.v4.widget.DrawerLayout;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.crystal.crystalrangeseekbar.interfaces.OnRangeSeekbarChangeListener;
import com.crystal.crystalrangeseekbar.widgets.CrystalRangeSeekbar;
import com.example.eryk.aircar.Car;
import com.example.eryk.aircar.MainActivity;
import com.example.eryk.aircar.R;
import com.example.eryk.aircar.SearchResult;

import java.io.Serializable;
import java.util.Calendar;

/**
 * Created by Eryk on 20/11/2016.
 */


public class SearchFragment extends Fragment {
    //region Variables
    private View layout;
    // Variables for range seek bar
    private TextView minimumPriceTextView;
    private TextView maximumPriceTextView;
    private TextView carHeader;
    private TextView datesTextView;
    private TextView priceTextView;
    private CrystalRangeSeekbar rangeSeekbar;

    // Variables for date picker
    @SuppressLint("StaticFieldLeak")
    private static Button chooseFromDatebtn;
    @SuppressLint("StaticFieldLeak")
    private static Button chooseToDatebtn;
    private static final int TAG_1 = 1;
    private static final int TAG_0 = 0;
    public static int DIALOG_ID = 0;
    private static int from_y, from_m, from_d = 0;   // Stores TO (year, month and day)
    private static int to_y, to_m, to_d = 0;        // stores FROM (year,month,day)
    // Variables for spinners
    Spinner model;
    Spinner make;
    //Variables for search
    private Button searchButton;
    private Button resetButton;
    private EditText locationText;
    private int counter = 0;
    //endregion


    public SearchFragment() {}

    // Accessed from my pager adapter
    public static SearchFragment getInstance(int position) {
        SearchFragment searchFragment = new SearchFragment();
        return searchFragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Nullable
    @Override
    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup
            container, @Nullable Bundle savedInstanceState) {

        layout = inflater.inflate(R.layout.layout_search, container, false); // inflate layout

        FloatingActionButton mFab = (FloatingActionButton) layout.findViewById(R.id.fab); // Floating Action Button
        mFab.setBackgroundTintList(ColorStateList.valueOf(getResources().getColor(R.color.white))); // Set the background to white
        mFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                MainActivity.openDrawer(); // on click open the drawer
            }
        });
        initialize();
        setSearchBox(); // call method
        return layout;
    }


    //region Search Layout

    private void initialize(){
        locationText = (EditText) layout.findViewById(R.id.layout_search_searchBox);
        searchButton = (Button) layout.findViewById(R.id.layout_search_searchButton);
        minimumPriceTextView = (TextView) layout.findViewById(R.id.layout_search_minimumTextView);
        maximumPriceTextView = (TextView) layout.findViewById(R.id.layout_search_maximumTextView);
        rangeSeekbar = (CrystalRangeSeekbar) layout.findViewById(R.id.layout_search_crystalRangeSeekbar);
        priceTextView = (TextView) layout.findViewById(R.id.layout_search_priceRangeText);
        resetButton = (Button) layout.findViewById(R.id.layout_search_resetButton);
        datesTextView = (TextView) layout.findViewById(R.id.layout_search_datesTextView);
        carHeader = (TextView) layout.findViewById(R.id.layout_search_chooseCarHeader);
        make = (Spinner) layout.findViewById(R.id.layout_search_makeSpinner);
        model = (Spinner) layout.findViewById(R.id.layout_search_modelSpinner);
        chooseFromDatebtn = (Button) layout.findViewById(R.id.layout_search_fromButton);
        chooseToDatebtn = (Button) layout.findViewById(R.id.layout_search_toButton);

        searchButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                switch (counter){ // check counter
                    case 0:
                        // if user did enter location then increment coutner and call setPrice()
                        if(!locationText.getText().toString().equals("")){
                            counter++;
                            setPrice();

                        }else{
                            Toast.makeText(getContext(), R.string.pleaseFillLocation, Toast.LENGTH_SHORT).show(); // display error msg
                        }
                        break;
                    case 1:
                        // increment coutner and call setDates()
                        counter++;
                        setDates();
                        break;
                    case 2:
                        // If user did enter dates and they are valid, then increment counter and call setMakeModel()
                        if(validateDates()){
                            counter++;
                            setMakeModel();
                        }else{
                            Toast.makeText(getContext(), R.string.datesBlank, Toast.LENGTH_SHORT).show(); // display error msg
                        }
                        break;
                    case 3:
                        startSearch(); // search

                }
            }
        });
    }

    private void setSearchBox(){
        // This method sets up the first "searchbox"
        resetFragment();
        // set views to visible
        locationText.setVisibility(View.VISIBLE);
        searchButton.setVisibility(View.VISIBLE);
        // animate alpha
        searchButton.animate().alpha(1).setDuration(1000).start();
        locationText.animate().alpha(1).setDuration(1000).start();
        // set onclick listener


    }

    private void setPrice(){
        resetFragment();
        // Set visibility
        minimumPriceTextView.setVisibility(View.VISIBLE);
        maximumPriceTextView.setVisibility(View.VISIBLE);
        rangeSeekbar.setVisibility(View.VISIBLE);
        priceTextView.setVisibility(View.VISIBLE);
        resetButton.setVisibility(View.VISIBLE);



        // Set listeners to the button and seekbar
        if (rangeSeekbar != null) {
            rangeSeekbar.setOnRangeSeekbarChangeListener(new OnRangeSeekbarChangeListener() {
                @Override
                public void valueChanged(Number minValue, Number maxValue) {
                    String price = "€ " + minValue.toString();
                    minimumPriceTextView.setText(price);
                    price = "€ " + maxValue.toString();
                    maximumPriceTextView.setText(price);
                }
            });
        }

        resetButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                counter = 0;
                setSearchBox(); // start searchbox
            }
        });

        // Animate alpha to 1
        minimumPriceTextView.animate().alpha(1).setDuration(1000).start();
        maximumPriceTextView.animate().alpha(1).setDuration(1000).start();
        rangeSeekbar.animate().alpha(1).setDuration(1000).start();
        priceTextView.animate().alpha(1).setDuration(1000).start();
        resetButton.animate().alpha(1).setDuration(1000).start();
    }

    private void setDates(){
        resetFragment();
        // Set visibility
        datesTextView.setVisibility(View.VISIBLE);
        chooseFromDatebtn.setVisibility(View.VISIBLE);
        chooseToDatebtn.setVisibility(View.VISIBLE);

        // Set onClick listeners
        chooseFromDatebtn.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View arg0) {
                DIALOG_ID = 0;
                DialogFragment newFragment = new SearchFragment.SelectDateFragment();
                newFragment.show(getFragmentManager(), "DatePicker");


            }
        });
        chooseToDatebtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                DIALOG_ID = 1;
                DialogFragment newFragment = new SearchFragment.SelectDateFragment();
                newFragment.show(getFragmentManager(),"DatePicker");

            }
        });

        // Animate alpha to 1 (in order to show)
        datesTextView.animate().alpha(1).setDuration(1000).start();
        chooseFromDatebtn.animate().alpha(1).setDuration(1000).start();
        chooseToDatebtn.animate().alpha(1).setDuration(1000).start();

    }

    private void setMakeModel(){
        resetFragment();
        //Animate alpha to 0
        chooseFromDatebtn.animate().alpha(0).setDuration(300).start();
        chooseToDatebtn.animate().alpha(0).setDuration(300).start();
        datesTextView.animate().alpha(0).setDuration(300).start();
        // Make it GONE
        chooseFromDatebtn.setVisibility(View.GONE);
        chooseToDatebtn.setVisibility(View.GONE);
        datesTextView.setVisibility(View.GONE);
        searchButton.setText(R.string.search);
        // Find views and spinners
        /*carHeader = (TextView) layout.findViewById(R.id.layout_search_chooseCarHeader);
        make = (Spinner) layout.findViewById(R.id.layout_search_makeSpinner);
        model = (Spinner) layout.findViewById(R.id.layout_search_modelSpinner);*/
        ArrayAdapter makeAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.makeArray,R.layout.support_simple_spinner_dropdown_item);
        ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.emptyArray,R.layout.support_simple_spinner_dropdown_item);
        model.setAdapter(modelAdapter); // set adapter to model
        make.setAdapter(makeAdapter);  // set adapter to make
        // initialize onItemselected
        make.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                if(make.getSelectedItem().equals("Alfa Romeo")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.alfaRomeoArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Audi")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.audiArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("BMW")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.bmwArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Fiat")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.fiatArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Ford")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.fordArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Honda")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.hondaArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Hyundai")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.hyundaiArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Lexus")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.lexusArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Mazda")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.mazdaArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Mercedes-Benz")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.mercedesArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Nissan")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.nissanArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Opel")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.openArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Peugeot")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.peugeotArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Seat")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.seatArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }else if(make.getSelectedItem().equals("Volvo")){
                    ArrayAdapter modelAdapter = ArrayAdapter.createFromResource(MainActivity.getActivityThis(),R.array.volvoArray,R.layout.support_simple_spinner_dropdown_item);
                    model.setAdapter(modelAdapter);
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });

        // Change the visibility to visible
        carHeader.setVisibility(View.VISIBLE);
        make.setVisibility(View.VISIBLE);
        model.setVisibility(View.VISIBLE);
        // Animate
        carHeader.animate().alpha(1).setDuration(1000).start();
        model.animate().alpha(1).setDuration(1000).start();
        make.animate().alpha(1).setDuration(1000).start();
    }

    private void startSearch(){
        // animate alpha to 0
        carHeader.animate().alpha(0).setDuration(300).start();
        model.animate().alpha(0).setDuration(300).start();
        make.animate().alpha(0).setDuration(300).start();
        // set visibility to gone
        carHeader.setVisibility(View.GONE);
        model.setVisibility(View.GONE);
        make.setVisibility(View.GONE);
        // create bundle and put all necessary information for SearchResult class to create query.
        Bundle bundle = new Bundle();
        bundle.putInt("dayFrom", from_d);
        bundle.putInt("monthFrom", from_m);
        bundle.putInt("yearFrom", from_y);
        bundle.putInt("dayTo", to_d);
        bundle.putInt("monthTo", to_m);
        bundle.putInt("yearTo", to_y);
        bundle.putString("location", locationText.getText().toString());
        int price = Integer.valueOf(minimumPriceTextView.getText().toString().substring(2));
        if(price == 0){
            price++;
        }
        bundle.putInt("priceFrom", price);
        price = Integer.valueOf(maximumPriceTextView.getText().toString().substring(2));
        if(price == 0){
            price++;
        }
        bundle.putInt("priceTo", price);
        bundle.putString("make", make.getSelectedItem().toString());
        bundle.putString("model", model.getSelectedItem().toString());

        // start searchResult class
        Intent i = new Intent(MainActivity.getContext(), SearchResult.class);
        i.putExtras(bundle);
        startActivity(i);

    }

    private void resetFragment(){
        // This methods resets fragment values by setting alpha to 0 to all objects and settings visibility to gone

        if(counter == 0) {
            from_m = from_y = from_d = to_d = to_m = to_y = 0;
        }

        if(carHeader.getVisibility() == View.VISIBLE){
            carHeader.animate().alpha(0).setDuration(300).start();
            carHeader.setVisibility(View.GONE);
        }
        if(model.getVisibility() == View.VISIBLE) {
            model.animate().alpha(0).setDuration(300).start();
            model.setVisibility(View.GONE);
        }
        if(make.getVisibility() == View.VISIBLE) {
            make.animate().alpha(0).setDuration(300).start();
            make.setVisibility(View.GONE);
        }
        if(chooseFromDatebtn.getVisibility() == View.VISIBLE) {
            chooseFromDatebtn.setText(R.string.dateFrom);
            chooseFromDatebtn.animate().alpha(0).setDuration(300).start();
            chooseFromDatebtn.setVisibility(View.GONE);
        }
        if(chooseToDatebtn.getVisibility() == View.VISIBLE) {
            chooseToDatebtn.setText(R.string.dateTo);
            chooseToDatebtn.animate().alpha(0).setDuration(300).start();
            chooseToDatebtn.setVisibility(View.GONE);
        }
        if(datesTextView.getVisibility() == View.VISIBLE) {
            datesTextView.animate().alpha(0).setDuration(300).start();
            datesTextView.setVisibility(View.GONE);
        }
        if(minimumPriceTextView.getVisibility() == View.VISIBLE) {
            minimumPriceTextView.animate().alpha(0).setDuration(300).start();
            minimumPriceTextView.setVisibility(View.GONE);
        }
        if(maximumPriceTextView.getVisibility() == View.VISIBLE) {
            maximumPriceTextView.animate().alpha(0).setDuration(300).start();
            maximumPriceTextView.setVisibility(View.GONE);
        }
        if(rangeSeekbar.getVisibility() == View.VISIBLE) {
            rangeSeekbar.animate().alpha(0).setDuration(300).start();
            rangeSeekbar.setVisibility(View.GONE);
        }
        if(priceTextView.getVisibility() == View.VISIBLE) {
            priceTextView.animate().alpha(0).setDuration(300).start();
            priceTextView.setVisibility(View.GONE);
        }
        if(locationText.getVisibility() == View.VISIBLE) {
            if(counter == 0) {
                locationText.setText("");
            }
            locationText.animate().alpha(0).setDuration(300).start(); // Animate alpha
            locationText.setVisibility(View.GONE);
        }
        if(counter != 3){
            searchButton.setText(R.string.next);
        }
    }

    //endregion

    // Method to validate Dates
    private boolean validateDates(){
        return !(from_m == 0 || from_d == 0 || from_y == 0 || to_y == 0 || to_m == 0 || to_d == 0);
    }

    public static void setDate(int year, int month, int day){
        // method used to set the date in searchFragment
        if(DIALOG_ID == TAG_0){
            from_y = year;
            from_m = month;
            from_d = day;
        }else if(DIALOG_ID == TAG_1){
            to_y = year;
            to_m = month;
            to_d = day;
        }
    }


    /*
            This class creates the DatePickerDialog
    */

    public static class SelectDateFragment extends DialogFragment implements DatePickerDialog.OnDateSetListener {

        @NonNull
        @Override
        public Dialog onCreateDialog(Bundle savedInstanceState) {
            final Calendar calendar = Calendar.getInstance();
            int yy = calendar.get(Calendar.YEAR);
            int mm = calendar.get(Calendar.MONTH);
            int dd = calendar.get(Calendar.DAY_OF_MONTH);
            return new DatePickerDialog(getActivity(), this, yy, mm, dd);
        }

        /*
            Handles the result of the calendar dialog (the date that user chooses).
            This method checks if the user didn't choose a date before today, if not then the date will be saved in the variables.
            Then populateSetDate() method is called.
         */
        @Override
        public void onDateSet(DatePicker view, int yy, int mm, int dd) {
            if(checkCurrentDate(yy,mm,dd)){
                SearchFragment.setDate(yy,mm,dd);
                populateSetDate(yy, mm + 1, dd);

                if(DIALOG_ID == 4){ // Once dialogid = 4 that means user have choosen TO date in mapFragment
                    startCarClass();
                }

                if(DIALOG_ID == 3){ // If user is choosing TO date in mapFragment then increment the dialog id
                    DIALOG_ID++;
                }
            }else{
                Toast.makeText(MainActivity.getActivityThis(), R.string.dateBeforeToday, Toast.LENGTH_SHORT).show();
            }
        }

        /*This method checks if the selected date by the user is smaller than actual date, if so it will return false
            otherwise it returns true
         */
        private boolean checkCurrentDate(int y, int m, int d){
            Calendar calendar = Calendar.getInstance();
            int current_y = calendar.get(Calendar.YEAR);
            int current_m = calendar.get(Calendar.MONTH);
            int current_d = calendar.get(Calendar.DAY_OF_MONTH);

            if(y < current_y){
                return false;
            }else if(m < current_m){
                return false;
            }else if(d < current_d){
                return false;
            }
            return true;
        }

        /*
            This method includes error checking to make sure that the return date is not before the rental date.
            If everything is successful, feedback is provided to the user i.e. the button changes to the date specified.
            If something goes wrong, user will be notified using a Toast.
         */
        public void populateSetDate(int year, int month, int day) {
            if(DIALOG_ID == 0){
                if(to_y != 0){
                    if(from_y > to_y || from_m > to_m || from_d > to_d){
                        Toast.makeText(MainActivity.getActivityThis(), R.string.calendar_error_from_after_to, Toast.LENGTH_SHORT).show();
                    }else{
                        chooseFromDatebtn.setText("From: " + month + "/" + day + "/" + year);
                    }
                }else {
                    chooseFromDatebtn.setText("From: " + month + "/" + day + "/" + year);
                }
            }else if(DIALOG_ID == 1){
                if(from_y != 0){
                    if(to_y < from_y || from_m > to_m || from_d > to_d){
                        Toast.makeText(MainActivity.getActivityThis(), R.string.calendar_error_to_before_from, Toast.LENGTH_SHORT).show();
                    }else{
                        chooseToDatebtn.setText("To: " + month + "/" + day + "/" + year);
                    }
                }else {
                    chooseToDatebtn.setText("To: " + month + "/" + day + "/" + year);
                }
            }else if(DIALOG_ID == 2 || DIALOG_ID == 3){ // Means class was called from map fragment
                Calendar calendar = MapFragment.getFromCalendar(); // Get from calendar
                Calendar toCalendar = MapFragment.getToCalendar(); // Get to calendar
                // Get the individual fields.
                int yy = calendar.get(Calendar.YEAR);
                int mm = calendar.get(Calendar.MONTH);
                int dd = calendar.get(Calendar.DAY_OF_MONTH);

                int ty = toCalendar.get(Calendar.YEAR);
                int tm = toCalendar.get(Calendar.MONTH);
                int td = toCalendar.get(Calendar.DAY_OF_MONTH);

                if(DIALOG_ID == 2) { // class is dealing with FROM date
                    if (ty != 0) {
                        if (yy > ty || mm > tm || dd > td) {
                            Toast.makeText(MainActivity.getActivityThis(), R.string.calendar_error_from_after_to, Toast.LENGTH_SHORT).show();
                        }else {
                            DIALOG_ID = 3;
                            MapFragment.setFromCalendar(year,month,day);
                            DialogFragment newFragment = new SearchFragment.SelectDateFragment();
                            newFragment.show(getFragmentManager(), "Date Picker");
                            Toast.makeText(getContext(), "CHOSE TO DATE", Toast.LENGTH_SHORT).show();
                        }
                    } else {
                        DIALOG_ID = 3;
                        MapFragment.setFromCalendar(year,month,day);
                        DialogFragment newFragment = new SearchFragment.SelectDateFragment();
                        newFragment.show(getFragmentManager(), "Date Picker");
                        Toast.makeText(getContext(), "CHOSE TO DATE", Toast.LENGTH_SHORT).show();
                        MapFragment.setFromCalendar(year,month,day);
                    }
                }if(DIALOG_ID == 3){ // class is dealing with TO date
                    if (yy != 0) {
                        if (yy > ty || mm > tm || dd > td) {
                            Toast.makeText(MainActivity.getActivityThis(), R.string.calendar_error_to_before_from, Toast.LENGTH_SHORT).show();
                        }else {
                            MapFragment.setToCalendar(year,month,day);

                        }
                    } else {
                        MapFragment.setToCalendar(year,month,day);
                    }
                }
            }
        }

        private void startCarClass(){
            // This method starts car class
            Intent i = new Intent(getContext(),Car.class);
            i.putExtra(MainActivity.TAGS.POSTID_TAG,MapFragment.getCurrentMarkerString());
            i.putExtra(MainActivity.TAGS.RENTFROM_TAG,(Serializable) MapFragment.getFromCalendar());
            i.putExtra(MainActivity.TAGS.RENTTO_TAG, (Serializable) MapFragment.getToCalendar());
            i.putExtra(MainActivity.TAGS.SHOW_RESERVE_TAG,true);
            i.putExtra(MainActivity.TAGS.BACKCHANGED_TAG,true);
            startActivity(i);
        }
    }
    //endregion

}



