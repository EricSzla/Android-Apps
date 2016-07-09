package erpam.favouriteplaces;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.location.Address;
import android.location.Criteria;
import android.location.Geocoder;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.provider.Settings;
import android.support.v4.app.ActivityCompat;
import android.support.v4.app.FragmentActivity;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.MenuItem;
import android.widget.Toast;

import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MarkerOptions;

import java.io.IOException;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import static erpam.favouriteplaces.MainActivity.*;

public class MapsActivity extends AppCompatActivity implements OnMapReadyCallback, GoogleMap.OnMapLongClickListener, LocationListener {

    private GoogleMap mMap;
    int locations = -1;
    LocationManager locationManager;
    String provider = "";

    @Override
    public void onMapLongClick(LatLng point) {

        Geocoder geocoder = new Geocoder(getApplicationContext(), Locale.getDefault());
        String label = new Date().toString();

        try {
            List<Address> addressList = geocoder.getFromLocation(point.latitude, point.longitude, 1);
            if (addressList != null && addressList.size() > 0) {
                label = addressList.get(0).getAddressLine(0);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }

        SharedPreferences sharedPreferences = this.getSharedPreferences("erpam.favouriteplaces",Context.MODE_PRIVATE);

        MainActivity.places.add(label);
        MainActivity.arrayAdapter.notifyDataSetChanged();
        MainActivity.locations.add(point);
        MainActivity.arraySize = MainActivity.arraySize + 1;
        Log.i("Array Size in Map: ", Integer.toString(arraySize));

        sharedPreferences.edit().putInt("arraySize:", MainActivity.arraySize).apply();
        sharedPreferences.edit().putString("places: " + Integer.toString(MainActivity.arraySize), MainActivity.places.get(MainActivity.arraySize).toString()).apply();
        sharedPreferences.edit().putFloat("lat " + Integer.toString(MainActivity.arraySize), ((float)point.latitude)).apply();
        sharedPreferences.edit().putFloat("lng " + Integer.toString(MainActivity.arraySize), ((float)point.longitude)).apply();



        mMap.addMarker(new MarkerOptions().position(point).title(label).icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_RED)));
    }



    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_maps);
        // Obtain the SupportMapFragment and get notified when the map is ready to be used.
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager()
                .findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);

        locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        provider = locationManager.getBestProvider(new Criteria(), false);

        Intent i = getIntent();
        locations = i.getIntExtra("Address", -1);


        if(provider !=null && locationManager.isProviderEnabled(provider)){

        }else{
            Toast.makeText(this, "Please turn on location services", Toast.LENGTH_LONG).show();
            Intent myIntent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
            this.startActivity(myIntent);
        }
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;
        mMap.setOnMapLongClickListener(this);

        if (locations != -1 && locations != 0) {
            locationManager.removeUpdates(this);
            mMap.moveCamera(CameraUpdateFactory.newLatLng(MainActivity.locations.get(locations)));
            mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(MainActivity.locations.get(locations), 10));
            mMap.addMarker(new MarkerOptions().position(MainActivity.locations.get(locations)).title(places.get(locations).toString()));
        } else {
            if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
            locationManager.requestLocationUpdates(provider, 400, 1, this);
        }
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()){
            case android.R.id.home:
                if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return true; }
                locationManager.removeUpdates(this);
                this.finish();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }

    }

    @Override
    public void onLocationChanged(Location location) {
        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(location.getLatitude(),location.getLongitude()),10));
    }

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
    protected void onResume() {
        super.onResume();

        if (locations == -1 || locations == 0) {
            if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
            locationManager.requestLocationUpdates(provider, 100, 1, this);
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
        locationManager.removeUpdates(this);
    }
}
