package com.example.eryk.aircar;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.res.Resources;
import android.net.Uri;
import android.os.Bundle;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.ContextMenu;
import android.view.View;
import android.support.design.widget.NavigationView;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.MenuItem;
import android.widget.AdapterView;
import android.widget.TextView;
import android.widget.Toast;

import com.example.eryk.aircar.fragments.FavouriteFragment;
import com.example.eryk.aircar.tabs.MyPagerAdapter;
import com.example.eryk.aircar.tabs.SlidingTabLayout;
import com.parse.DeleteCallback;
import com.parse.GetCallback;
import com.parse.LogOutCallback;
import com.parse.ParseException;
import com.parse.ParseObject;
import com.parse.ParseQuery;
import com.parse.ParseUser;

/*
    Created by Eryk on 19/11/16.
    The main class
 */
public class MainActivity extends AppCompatActivity
        implements NavigationView.OnNavigationItemSelectedListener {

    private static Activity activityThis;   // Class Variable that stores Activity.
    private static Context activityContext; // Class variable that stores context.
    private static DrawerLayout drawer;     // Class variable that stores drawer layout.
    private TextView name;                  // Name for the drawer.

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_navigation);
        activityThis = this;                        // Initialize actiityThis
        activityContext = getApplicationContext();  // Initialize context

        setNavigationDrawer();                      // Call setNavigationDrawer method

        initToolbars();                             // Call InitToolbars method
    }


    private void initToolbars() {
        /*
            Method used to initialize ViewPager and slidingTabLayout
        */

        ViewPager viewPager = (ViewPager) findViewById(R.id.mainSwapView);                             // Find viewPager
        viewPager.setAdapter(new MyPagerAdapter(getSupportFragmentManager(),getApplicationContext())); // Set the viewPager's adapter to MyPagerAdapter
        viewPager.setOffscreenPageLimit(3);                                                            // Set off screen page limit to 3

        SlidingTabLayout slidingTabLayout = (SlidingTabLayout) findViewById(R.id.tabs);                // Find slidingTabLayout
        slidingTabLayout.setDistributeEvenly(true);                                                    // Distribute it evenly
        slidingTabLayout.setCustomTabView(R.layout.custom_tab_view, R.id.tabText);                     // Set the tabview

        slidingTabLayout.setBackgroundColor(getResources().getColor(R.color.main_blue));               // Set bg color
        slidingTabLayout.setViewPager(viewPager);                                                      // SET VIEW PAGER
    }


    @Override
    public void onBackPressed() {
        // Overide the onBackPressed button
        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout); // Find drawer
        if (drawer.isDrawerOpen(GravityCompat.START)) {                        // If the drawer is opened
            drawer.closeDrawer(GravityCompat.START);                           // Then close it
        } else {
            logout();                                                          // Otherwise continue to logOut() (which will ask the user if the user is sure to log out).
        }
    }

    private void logout(){
        // Create new dialog interface listener
        DialogInterface.OnClickListener dialogClickListener = new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                switch (which){
                    case DialogInterface.BUTTON_POSITIVE: // if positive was choosen
                        ParseUser.logOutInBackground(new LogOutCallback() {
                            @Override
                            public void done(ParseException e) { // initialize logout
                                if(e == null){ // if no errors
                                    // go back to login activity
                                    Intent i = new Intent(getApplicationContext(),LoginActivity.class);
                                    startActivity(i);
                                }else{
                                    // display error msg
                                    Toast.makeText(MainActivity.getActivityThis(), R.string.failedToLogOut, Toast.LENGTH_SHORT).show();
                                }
                            }
                        });
                        break;

                    case DialogInterface.BUTTON_NEGATIVE:
                        //Keep searching button clicked
                        break;
                }
            }
        };

        AlertDialog.Builder builder = new AlertDialog.Builder(this); // builder
        builder.setMessage(R.string.areYouSure).setPositiveButton(R.string.logOut, dialogClickListener) // set msgs
                .setNegativeButton(R.string.keepSearching, dialogClickListener).show(); // and show
    }

    private void email(){
        /*
            Email method that starts standard email application with headers and body already filled in.
         */
        String[] CC = {ParseUser.getCurrentUser().getEmail()}; // set user's email ('FROM' part of email)
        String[] TO = {getResources().getString(R.string.erpamEmail)};           // set TO email
        Intent emailIntent = new Intent(Intent.ACTION_SEND);   // initialize intent
        emailIntent.setData(Uri.parse("mailto:"));             // set data mailto:
        emailIntent.setType("text/plain");                     // set type

        String msg = getResources().getString(R.string.hey) +" \r\n"+ getResources().getString(R.string.feedbackBody) +"\r\n\r\n"; // body msg
        emailIntent.putExtra(Intent.EXTRA_EMAIL, TO); // set TO
        emailIntent.putExtra(Intent.EXTRA_CC, CC);    // set CC (from)
        emailIntent.putExtra(Intent.EXTRA_SUBJECT, getResources().getString(R.string.feedback)); // Set subject
        emailIntent.putExtra(Intent.EXTRA_TEXT, msg + "\r\n\r\n" + // SET BODY
                "\r\n\r\n"+ getResources().getString(R.string.regards) +"\r\n" + ParseUser.getCurrentUser().getEmail() +
                "\r\n\r\n"
                +getResources().getString(R.string.mailGenerated));

        try {
            startActivity(Intent.createChooser(emailIntent, "Send mail...")); // start Email
            Log.i("Finished sending email", "");
        } catch (android.content.ActivityNotFoundException ex) {
            // Display error (no client installed)
            Toast.makeText(this,R.string.noClient, Toast.LENGTH_SHORT).show();
        }
    }

    private void startMainActivity(){
        // Method to restart main activity
        Log.i("MAIN: ", "RESTARTING");
        Intent i = new Intent(this,MainActivity.class);
        startActivity(i);
    }

    private void deleteFavouritePost(int pos){
        // Method responsible for deleting the favouite post

        ParseQuery<ParseObject> deleteQuery = ParseQuery.getQuery(TAGS.FAVOURITE);           // New query (favourite collection)
        Log.i("ID: ", FavouriteFragment.getId(pos));
        deleteQuery.whereEqualTo(MainActivity.TAGS.POSTID_TAG,FavouriteFragment.getId(pos)); // where POSTID = clicked post's id
        deleteQuery.whereEqualTo(TAGS.USERID_TAG,ParseUser.getCurrentUser().getObjectId());  // where USERID = user's id
        deleteQuery.getFirstInBackground(new GetCallback<ParseObject>() {
            @Override
            public void done(ParseObject object, ParseException e) {            // get first in background
                if(e == null){ // if no exceptions
                    try {
                        object.deleteInBackground(new DeleteCallback() {
                            @Override
                            public void done(ParseException e) { // delete
                                if(e == null){ // if success
                                    setContentView(R.layout.activity_main); // change back the content view and inform user
                                    Toast.makeText(getActivityThis(), R.string.postDeleted, Toast.LENGTH_SHORT).show();
                                    startMainActivity(); // restart activity to refresh the posts

                                }else{
                                    setContentView(R.layout.activity_main); // set back the content and inform user about the failure
                                    Toast.makeText(getActivityThis(), R.string.postNotDeleted, Toast.LENGTH_SHORT).show();
                                }
                            }
                        });
                    }catch (Exception exe){
                        exe.printStackTrace(); // print exception
                        setContentView(R.layout.activity_main); // set back content and inform user
                        Toast.makeText(getActivityThis(), R.string.postNotDeleted, Toast.LENGTH_SHORT).show();
                    }
                }else{
                    Log.i("MAIN: ", "e != null");
                    e.printStackTrace(); // set back content and inform user
                    setContentView(R.layout.activity_main);
                    Toast.makeText(getActivityThis(), R.string.postNotDeleted, Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    //region getters
    public static Activity getActivityThis() {return activityThis;}
    public static Context getContext() {return activityContext;}
    //endregion

    //region contextMenu
    // Method used for history fragment
    // Adds "View" and "Delete" to the menu
    public void onCreateContextMenu(ContextMenu menu, View v, ContextMenu.ContextMenuInfo menuInfo) {
        menu.add(0, v.getId(), 0, "View");          // Add "View"
        menu.add(1, v.getId(), 1, "Delete");        // Add "Delete"
        Log.i("IDs: ",String.valueOf(v.getId()));
    }

    // Method to handle the action when user selects "View" or "Delete" in favouite fragment
    @Override
    public boolean onContextItemSelected(MenuItem item) {
        AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) item.getMenuInfo(); // get info
        int index = info.position; // get position
        Log.i("INDEX: ", String.valueOf(info.position));
        Log.i("Item Id: ", String.valueOf(item.getItemId()));
        Log.i("Group id: ", String.valueOf(item.getGroupId()));
        setContentView(R.layout.progress_bar); // set content view to progress bar
        switch (item.getGroupId()){
            case 0: // "View" was selected.
                // Start activity car
                Intent i = new Intent(getContext(),Car.class);
                String id = FavouriteFragment.getId(index);
                i.putExtra(MainActivity.TAGS.POSTID_TAG, id); // put post id
                i.putExtra(MainActivity.TAGS.SHOW_RESERVE_TAG,false); // put boolean ( to prevent showing "reserve button" )
                startActivity(i);
                break;
            case 1: // Delete was selected.
                Log.i("Main: ", "DELETING POST");
                deleteFavouritePost(index); // Delete posts
                break;
        }
        return super.onContextItemSelected(item);
    }
    //endregion

    //region drawer

    /*
        Method used to initialize the navigation drawer .
     */
    private void setNavigationDrawer(){
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);     // Find toolbar
        drawer = (DrawerLayout) findViewById(R.id.drawer_layout);   // Find drawer
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(   // Initialize new ActionBarDrawerToggle
                this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close){
            @Override
            public void onDrawerOpened(View drawerView) { // Overide onDrawerOpened method
                invalidateOptionsMenu();
                name = (TextView) findViewById(R.id.emailTextView);  // When opened, find name textView and initialize it
                name.setText(ParseUser.getCurrentUser().getUsername()); // to user's username (same as emial).
            }
        };
        drawer.setDrawerListener(toggle);
        toggle.syncState();

        NavigationView navigationView = (NavigationView) findViewById(R.id.nav_view);  // Find view
        navigationView.setNavigationItemSelectedListener(this);                        // SetListener
    }

    // Method to open the drawer
    public static void openDrawer(){
        drawer.openDrawer(GravityCompat.START);
    }

    // Method used for navigation drawer, to handle the action when user chooses something
    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(MenuItem item) {
        // Handle navigation view item clicks here.
        int id = item.getItemId();
        if (id == R.id.nav_update) {
            // If user chooses to update details, update class will be started.
            Intent i = new Intent(getContext(),UpdateDetails.class);
            startActivity(i);
        } else if (id == R.id.nav_logout) {
            // If user chooses to logout, logout will be called.
            Log.i("MAIN","LogOut");
            logout();
        }else if(id == R.id.nav_feedback){
            // If user wants to give feedback, email will be called.
            Log.i("MAIN","FeedBack");
            email();
        }

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout); // find drawer
        drawer.closeDrawer(GravityCompat.START); // close drawer
        return true;
    }
    //endregion

    public static class TAGS{
        // Strings for database (Added to the code instead of strings.xml for security reasons).
        public static final String CAR_PICTURE = "pic1";
        public static final String CAR_PICTURE0 = "pic";
        public static final String DESCRIPTION = "description";
        public static final String CAR_POST = "carPost";
        public static final String FAVOURITE = "Favourite";
        public static final String OBJECTID_TAG = "objectId";
        public static final String URL_TAG = "url";

        public static final String HISTORY_TAG = "History";
        public static final String USERID_TAG  = "userId";
        public static final String POSTID_TAG = "postId";
        public static final String RENTFROM_TAG = "rentFrom";
        public static final String RENTTO_TAG = "rentTo";
        public static final String CARPOST_TAG = "carPost";
        public static final String DESCRIPTION_TAG = "description";

        public static final String AFROM_TAG = "availableFrom";
        public static final String ATO_TAG = "availableTo";
        public static final String CITY_TAG = "city";
        public static final String PRICE_TAG = "priceDay";
        public static final String ANYMAKE_TAG = "any make";
        public static final String ANYMODEL_TAG = "any model";
        public static final String MODEL_TAG = "model";
        public static final String MAKE_TAG = "make";
        public static final String REG_TAG = "reg";
        public static final String ENGINE_TAG = "engine";
        public static final String POSTED_TAG = "posted";
        public static final int ONE_TAG = 1;
        public static final int ZERO_TAG = 0;
        public static final String NOTIFICATIONS_TAG = "Notifications";
        public static final String CUSTOMERID_TAG ="customerId";
        public static final String POSTERID_TAG ="posterId";
        public static final String MESSAGE_TAG ="message";
        public static final String EXPERIENCE_TAG ="experience";
        public static final String SHOW_RESERVE_TAG ="showReserve";

        public static final int SIX_TAG = 6;
        public static final String ONE_STRING_TAG = "1";
        public static final String LOCATION_TAG = "location";
        public static final String LATITUDE_TAG ="latitude";
        public static final String LONGITUDE_TAG = "longitude";
        public static final String EMAIL_TAG = "email";
        public static final String BACKCHANGED_TAG ="changeBack";
    }
}
