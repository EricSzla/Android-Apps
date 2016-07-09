package erpam.hikersapp;

import android.Manifest;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.location.Address;
import android.location.Criteria;
import android.location.Geocoder;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.provider.Settings;
import android.support.v4.app.ActivityCompat;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;
import android.widget.Toast;

import org.w3c.dom.Text;

import java.io.IOException;
import java.util.List;
import java.util.Locale;

public class MainActivity extends Activity implements LocationListener {

    LocationManager locationManager;
    String provider;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        provider = locationManager.getBestProvider(new Criteria(), false);

        if(provider !=null && locationManager.isProviderEnabled(provider)){

        }else{
            Toast.makeText(this, "Please turn on location services", Toast.LENGTH_LONG).show();
            Intent myIntent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
            this.startActivity(myIntent);
        }
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
        else {
            Location location = (Location) locationManager.getLastKnownLocation(provider);
            if(location != null) {
                onLocationChanged(location);
            }
        }


    }

    @Override
    protected void onPause() {
        super.onPause();

        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
        else {
            locationManager.removeUpdates(this);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();

        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {if (ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_FINE_LOCATION) && ActivityCompat.shouldShowRequestPermissionRationale(this, android.Manifest.permission.ACCESS_COARSE_LOCATION)) {Log.i("info", "need to ask permission since it is note accepted in the first time");ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);} else {ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 0);Log.i("info", "if statement working, permission yet not granted");}return; }
        else {
            locationManager.requestLocationUpdates(provider, 400, 1, this);
        }
    }

    @Override
    public void onLocationChanged(Location location) {
        Double lat = location.getLatitude();
        Double lng = location.getLongitude();
        Double alt = location.getAltitude();
        Float acc = location.getAccuracy();
        Float bearing = location.getBearing();
        Float speed = location.getSpeed();

        Geocoder geocoder = new Geocoder(getApplicationContext(), Locale.getDefault());
        TextView address = (TextView) findViewById(R.id.address);

        try {
            List<Address> addressList = geocoder.getFromLocation(lat, lng, 1);

            if(addressList != null && addressList.size() > 0) {
                Log.i("Adress: ", addressList.get(0).toString());
                String addressHolder ="";

                for(int i =0; i < addressList.get(0).getMaxAddressLineIndex(); i++) {
                    addressHolder += addressList.get(0).getAddressLine(i).toString();
                    addressHolder += " ";
                }

                address.setText("Address: " + addressHolder);


            }
        } catch (IOException e) {
            e.printStackTrace();
        }

        Log.i("Lat: ", String.valueOf(lat));
        Log.i("Lng: ", String.valueOf(lng));
        Log.i("Alt: ", String.valueOf(alt));
        Log.i("Acc: ", String.valueOf(acc));
        Log.i("Ber: ", String.valueOf(bearing));
        Log.i("Spe: ", String.valueOf(speed));

        TextView altView = (TextView)findViewById(R.id.altitude);
        TextView latView = (TextView)findViewById(R.id.lat);
        TextView lngView = (TextView)findViewById(R.id.lng);
        TextView accView = (TextView)findViewById(R.id.accuracy);
        TextView berView = (TextView)findViewById(R.id.bearing);
        TextView speView = (TextView)findViewById(R.id.speed);

        altView.setText("Altitude: " + alt.toString());
        latView.setText("Latitude: " +lat.toString());
        lngView.setText("Longitude: " + lng.toString());
        accView.setText("Accuracy: " + acc.toString());
        berView.setText("Bearing: " + bearing);
        speView.setText("Speed: " + speed.toString());



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
}
