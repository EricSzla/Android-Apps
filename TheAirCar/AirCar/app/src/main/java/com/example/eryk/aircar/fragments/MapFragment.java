package com.example.eryk.aircar.fragments;

import android.Manifest;
import android.app.Fragment;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.PorterDuff;
import android.graphics.drawable.Drawable;
import android.location.Address;
import android.location.Criteria;
import android.location.Geocoder;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.provider.Settings;
import android.support.annotation.DrawableRes;
import android.support.annotation.Nullable;
import android.support.v4.app.ActivityCompat;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.FragmentActivity;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.example.eryk.aircar.Car;
import com.example.eryk.aircar.MainActivity;
import com.example.eryk.aircar.R;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.MapView;
import com.google.android.gms.maps.MapsInitializer;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.CameraPosition;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.vision.text.Text;
import com.parse.FindCallback;
import com.parse.GetDataCallback;
import com.parse.ParseException;
import com.parse.ParseFile;
import com.parse.ParseGeoPoint;
import com.parse.ParseObject;
import com.parse.ParseQuery;

import java.io.IOException;
import java.io.InputStream;
import java.io.Serializable;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;

/**
 * Created by Eryk on 20/11/2016.
 * Class to deal with MapFragment
 */

public class MapFragment extends android.support.v4.app.Fragment implements OnMapReadyCallback, LocationListener{

    private GoogleMap mMap;                     // variable for map
    private LocationManager locationManager;    // locationManager
    private String provider;                    // provider i.e. gps
    Location location;                          // device location
    private ParseGeoPoint geoPoint;             // geoPoint (database)
    private ParseGeoPoint[] geoPoints;          // geoPoint array
    private Bitmap images[];                    // array of images
    private boolean notFetched = true;
    private List<HashMap<String, String>> cars; // stores cars
    private static String currentMarkerString = "";

    // Calendars (If user wants to open a car, the dates are needed).
    private static Calendar fromCalendar;
    private static Calendar toCalendar;
    // Called from mypagerAdapter
    public static MapFragment getInstance(int position) {
        return new MapFragment();
    }

    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.activity_maps, container, false); // inflate

        SupportMapFragment mapFragment = (SupportMapFragment) getChildFragmentManager() // map fragment
                .findFragmentById(R.id.mygooglemap);
        mapFragment.getMapAsync(this); // get the map
        notFetched = true; // set boolean to true
        fromCalendar = Calendar.getInstance(); // initialize calendars
        toCalendar = Calendar.getInstance();
        return rootView;
    }

    //Getters
    public static Calendar getFromCalendar(){return fromCalendar;}
    public static Calendar getToCalendar(){return toCalendar;}
    public static String getCurrentMarkerString(){return currentMarkerString;}
    //Setters
    public static void setFromCalendar(int y, int m, int d){
        fromCalendar.set(y,m,d);
    }
    public static void setToCalendar(int y, int m, int d){
        toCalendar.set(y,m,d);
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        // Method called when map is ready.
        mMap = googleMap; // initialize global map variable

        locationManager = (LocationManager) getActivity().getSystemService(Context.LOCATION_SERVICE); // get the location service
        provider = locationManager.getBestProvider(new Criteria(), false); // get the best provider (e.g. gps)

        // Check if provider is enabled
        if (provider != null && locationManager.isProviderEnabled(provider)) {
            requestLocation(); // request location
        } else { // If not, ask the user to turn on location services
            Toast.makeText(getContext(), R.string.turnOnLocation, Toast.LENGTH_LONG).show();
            Intent myIntent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS); // Open location settings
            this.startActivity(myIntent);
        }

        // Check for permissions
        if (ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            if (ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION)) {
                Log.i("info", "need to ask permission since it is note accepted in the first time");
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
            } else {
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
                Log.i("info", "if statement working, permission yet not granted");
            }
        }
        location = locationManager.getLastKnownLocation(provider); // get the location
        if (location == null) { // Check if there is any location
            requestLocation(); // If there is no location, then request it.
            Log.i("MAP", "Location is null");
        }else{
            onLocationChanged(location);
        }

        //LatLng latLng = new LatLng(53,-6);
        Location locations = new Location(provider);
        locations.setLatitude(53.348429);
        locations.setLongitude(-6.267684);
        onLocationChanged(locations);
        //fetchPosts(latLng);
    }

    private void requestLocation() {
        // Check permissions before requesting updates
        if (ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            if (ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION)) {
                Log.i("info", "need to ask permission since it is note accepted in the first time");
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
            } else {
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
                Log.i("info", "if statement working, permission yet not granted");
            }
            return;
        }
        if (locationManager != null) {
            locationManager.requestLocationUpdates(provider, 120000, 100, this);
        }

    }

    /*
        This method is called whenever the users location has been changed.
        It adds a new marker on the map with title "Your location".
        And fetches the posts.
     */

    @Override
    public void onLocationChanged(Location location) {
        // Add a marker in Sydney, Australia, and move the camera.
        LatLng userLocation = new LatLng(location.getLatitude(), location.getLongitude());
        this.location = location;

        mMap.moveCamera(CameraUpdateFactory.newLatLng(userLocation));
        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(location.getLatitude(), location.getLongitude()), 7));

        mMap.clear();
        mMap.addMarker(new MarkerOptions().position(userLocation).title(getResources().getString(R.string.yourLocation)));
        notFetched = !notFetched;
        fetchPosts(userLocation);

    }

    /*
        This method gets the posts from the database, it takes LatLng as parameter.
     */
    private void fetchPosts(LatLng location) {
        if (geoPoint == null) {
            geoPoint = new ParseGeoPoint(); // initialize geoPoint
        }
        Calendar calendar = Calendar.getInstance(); // get current date

        geoPoint.setLatitude(location.latitude);  // Set geoPoint's (type ParseGeoPoint) latitude.
        geoPoint.setLongitude(location.longitude); // Set geoPoint's longitude. (So it can be used in a query)
        ParseQuery<ParseObject> carQuery = new ParseQuery<>(MainActivity.TAGS.CAR_POST); // Create a query "carPost"
        carQuery.whereLessThanOrEqualTo(MainActivity.TAGS.AFROM_TAG,calendar.getTime());
        carQuery.whereGreaterThanOrEqualTo(MainActivity.TAGS.ATO_TAG,calendar.getTime());
        carQuery.whereWithinKilometers(MainActivity.TAGS.LOCATION_TAG, geoPoint, 300); // Set 'where' to look for cars in range of 300km from geoPoint
        carQuery.findInBackground(new FindCallback<ParseObject>() { // Find in background
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // Method called when query is done.
                if (e == null) { // Check for exception
                    if (objects.size() > 0) { // Check if any posts are found.
                        if (cars == null) { // Check if cars is null
                            cars = new ArrayList<>(); // If so, then initialize
                        }
                        Log.i("MAP", "Objects size: " + String.valueOf(objects.size()));
                        // initialize arrays
                        geoPoints = new ParseGeoPoint[objects.size()];
                        images = new Bitmap[objects.size()];
                        //Iterate through objects (carposts)
                        for (int i = 0; i < objects.size(); i++) {
                            //Create local hashmap to store data from the database
                            HashMap<String, String> localData = new HashMap<>();
                            localData.put(MainActivity.TAGS.POSTED_TAG, objects.get(i).get(MainActivity.TAGS.POSTED_TAG).toString());
                            localData.put(MainActivity.TAGS.POSTID_TAG, objects.get(i).getObjectId()); // Stores post id in localData hashmap
                            int price = objects.get(i).getInt(MainActivity.TAGS.PRICE_TAG); // Get int price
                            localData.put(MainActivity.TAGS.PRICE_TAG, String.valueOf(price)); // Store price (converted from int)
                            Log.i("MAP", "localPrice: " + localData.get(MainActivity.TAGS.PRICE_TAG));
                            geoPoints[i] = objects.get(i).getParseGeoPoint(MainActivity.TAGS.LOCATION_TAG); // Get location from database
                            localData.put(MainActivity.TAGS.LONGITUDE_TAG, String.valueOf(geoPoint.getLongitude())); // Store longitude in local hashmap
                            localData.put(MainActivity.TAGS.MAKE_TAG, objects.get(i).get(MainActivity.TAGS.MAKE_TAG).toString()); // Store car make
                            localData.put(MainActivity.TAGS.MODEL_TAG, objects.get(i).get(MainActivity.TAGS.MODEL_TAG).toString()); // Store car model
                            localData.put(MainActivity.TAGS.DESCRIPTION_TAG, objects.get(i).get(MainActivity.TAGS.DESCRIPTION_TAG).toString()); // Store description
                            ParseFile postImage = objects.get(i).getParseFile(MainActivity.TAGS.CAR_PICTURE); // Get the image from database
                            String imageUrl = postImage.getUrl(); // Get the image URL
                            localData.put(MainActivity.TAGS.URL_TAG, imageUrl); // Store the image in local hashmap
                            cars.add(i, localData); // Add local hashmap to global variable ArrayList<HashMap<String,String>>.
                            downloadImages(postImage, i, objects.size());
                        } // end of for loop
                        //initializeMarkers(); // After all the car posts are added to the ArrayList, call the initialize Markers
                    }
                }
            }
        });
    } // end of fetchPosts()

    /*
        This method will initialize markers on the map.
     */
    private void initializeMarkers() {
        // Iterate through the arraylist
        for (int i = 0; (i < cars.size() && i < geoPoints.length); i++) {
            if (cars.get(i).get(MainActivity.TAGS.POSTED_TAG).equals(MainActivity.TAGS.ONE_STRING_TAG)) {
                Double lat = geoPoints[i].getLatitude(); // get latitude
                Log.i("MAP", String.valueOf(lat));
                Double lng = geoPoints[i].getLongitude();   // get longitude
                LatLng userLocation = new LatLng(lat, lng); // set the LatLng
                String price = "€ " + cars.get(i).get(MainActivity.TAGS.PRICE_TAG); // Price string
                String header = cars.get(i).get(MainActivity.TAGS.MAKE_TAG).toUpperCase() + " "
                        + cars.get(i).get(MainActivity.TAGS.MODEL_TAG).toUpperCase() + " ," + price;

                // Create new marker (for each post i ).
                mMap.addMarker(new MarkerOptions().position(userLocation).title(header).icon(BitmapDescriptorFactory.fromBitmap(getMarkerBitmapFromView(images[i]))).snippet(
                        cars.get(i).get(MainActivity.TAGS.DESCRIPTION_TAG))
                ).setTag(cars.get(i).get(MainActivity.TAGS.POSTID_TAG)); // Create a marker

                // Initialize new on info window click listener.
                // This will check if user clicked on the "header" of the marker.
                // If so, this will redirect the user to the car activity to display appropriate car.
                mMap.setOnInfoWindowClickListener(new GoogleMap.OnInfoWindowClickListener() {
                    @Override
                    public void onInfoWindowClick(Marker marker) {
                        if(marker.getTag() != null) {
                            SearchFragment.DIALOG_ID = 2;
                            currentMarkerString = marker.getTag().toString();
                            DialogFragment newFragment = new SearchFragment.SelectDateFragment();
                            newFragment.show(getFragmentManager(), "Date Picker");
                            Toast.makeText(getContext(), "Choose FROM date.", Toast.LENGTH_SHORT).show();
                        }
                    }
                });

            }
        }
    }




    //region images
    /*
        This method returns a car image inside an imageView.
        Therefor a blue border is added to the image.
        Code source: http://stackoverflow.com/questions/14811579/how-to-create-a-custom-shaped-bitmap-marker-with-android-map-api-v2
     */
    private Bitmap getMarkerBitmapFromView(Bitmap image) {

        View customMarkerView = ((LayoutInflater) getActivity().getSystemService(Context.LAYOUT_INFLATER_SERVICE)).inflate(R.layout.layout_marker, null);
        ImageView markerImageView = (ImageView) customMarkerView.findViewById(R.id.profile_image);
        markerImageView.setImageBitmap(image);
        customMarkerView.measure(View.MeasureSpec.UNSPECIFIED, View.MeasureSpec.UNSPECIFIED);
        customMarkerView.layout(0, 0, customMarkerView.getMeasuredWidth(), customMarkerView.getMeasuredHeight());
        customMarkerView.buildDrawingCache();
        Bitmap returnedBitmap = Bitmap.createBitmap(customMarkerView.getMeasuredWidth(), customMarkerView.getMeasuredHeight(),
                Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(returnedBitmap);
        canvas.drawColor(Color.WHITE, PorterDuff.Mode.SRC_IN);
        Drawable drawable = customMarkerView.getBackground();
        if (drawable != null)
            drawable.draw(canvas);
        customMarkerView.draw(canvas);
        return returnedBitmap;
    }

    /*
        This method downloads the image from the ParseFile
        Inspiration for the code: http://stackoverflow.com/questions/24288466/retrieving-image-files-from-parse-android
    *
    * */
    private void downloadImages(ParseFile file, final int i, final int finalSize) {
        if (file != null) { // Check if file is not null
            Log.i("MAP", "Downloading image: " + i);
            Log.i("MAP", "FinalSize: " + finalSize);
            file.getDataInBackground(new GetDataCallback() { // Download in the background
                @Override
                public void done(byte[] data, ParseException e) { // When done
                    if (e == null) { // Check for exceptions
                        Bitmap bmp = BitmapFactory.decodeByteArray(data, 0, data.length); // store in a bitmap

                        if (bmp != null) { // if not null
                            Bitmap bitmap2 = Bitmap.createScaledBitmap(bmp, 240, 200, false);
                            images[i] = bitmap2; // then store in the global images[] bitmap

                            if (i == finalSize - 1) { // Check if i == finalSize if so, that means this is the last element
                                Log.i("MAP", "Initializing markers");
                                initializeMarkers(); // create markers on the map
                            }
                        }

                    } else {
                        e.printStackTrace(); // print exception stack trace
                    }
                }
            });
        }
    }

    //endregion

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) {

    }

    @Override
    public void onProviderEnabled(String provider) {

    }

    @Override
    public void onProviderDisabled(String provider) {

    }

    @Override
    public void onPause() {
        super.onPause();
        if (ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            if (ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(getActivity(), android.Manifest.permission.ACCESS_COARSE_LOCATION)) {
                Log.i("info", "need to ask permission since it is note accepted in the first time");
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
            } else {
                ActivityCompat.requestPermissions(getActivity(), new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);
                Log.i("info", "if statement working, permission yet not granted");
            }
        }
        if(locationManager != null) {
            locationManager.removeUpdates(this);
        }

    }

    @Override
    public void onResume() {
        super.onResume();
        requestLocation();
    }
}