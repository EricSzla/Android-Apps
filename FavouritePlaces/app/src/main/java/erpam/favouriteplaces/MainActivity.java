package erpam.favouriteplaces;

import android.app.ActionBar;
import android.app.Activity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.location.Location;
import android.os.Bundle;
import android.provider.Settings;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.ContextMenu;
import android.view.KeyEvent;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.view.inputmethod.InputMethodManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.Toast;

import com.google.android.gms.maps.model.LatLng;

import java.util.ArrayList;
import java.util.Set;


public class MainActivity extends AppCompatActivity {

    static ArrayList<String> places;
    static ArrayAdapter arrayAdapter;
    static ArrayList<LatLng> locations;
    int current = 0;
    static int arraySize = 0;

    public void onCreateContextMenu(ContextMenu menu, View v, ContextMenu.ContextMenuInfo menuInfo) {
        super.onCreateContextMenu(menu, v, menuInfo);
        if (v.getId()==R.id.listView) {
            MenuInflater inflater = getMenuInflater();
            inflater.inflate(R.menu.menu_list, menu);
        }



    }

    public void update(AdapterView.AdapterContextMenuInfo info){

        places.remove(info.position);
        locations.remove(info.position);
        arrayAdapter.notifyDataSetChanged();
        arraySize--;

        SharedPreferences sharedPreferences = this.getSharedPreferences("erpam.favouriteplaces", Context.MODE_PRIVATE);

        sharedPreferences.edit().putInt("arraySize:", MainActivity.arraySize).apply();

        for (int i = info.position; i <= arraySize; i++) {
            sharedPreferences.edit().putString("places: " + Integer.toString(i), sharedPreferences.getString("places: " + Integer.toString(i + 1), "")).apply();
            sharedPreferences.edit().putFloat("lat " + Integer.toString(i), sharedPreferences.getFloat("lat " + Integer.toString(i + 1), 0)).apply();
            sharedPreferences.edit().putFloat("lng " + Integer.toString(i), sharedPreferences.getFloat("lng " + Integer.toString(i + 1), 0)).apply();
        }
    }


    public boolean onContextItemSelected(MenuItem item) {
        final AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) item.getMenuInfo();
        switch(item.getItemId()) {
            case R.id.edit:
                if(info.position != 0) {
                    RelativeLayout layout = (RelativeLayout)findViewById(R.id.secondLay);
                    layout.setVisibility(View.VISIBLE);
                    layout.animate().alpha(0.8f).setDuration(400);
                    layout.forceLayout();
                    current = info.position;

                }else{
                    Toast.makeText(getApplicationContext(),"Can't edit this !",Toast.LENGTH_LONG).show();
                }
                return true;
            case R.id.delete:
                if(info.position != 0){

                    new AlertDialog.Builder(this)
                            .setIcon(android.R.drawable.ic_dialog_alert)
                            .setTitle("Are you sure?")
                            .setMessage("The deleted item cannot be restored.")
                            .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                            @Override
                            public void onClick(DialogInterface dialog, int which) {
                                update(info);
                                }
                            })
                            .setNegativeButton("No",null)
                            .show();
                }else {
                    Toast.makeText(getApplicationContext(),"Can't remove this !",Toast.LENGTH_LONG).show();
                }

                return true;
            default:
                return super.onContextItemSelected(item);
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        this.getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        ListView listView = (ListView) findViewById(R.id.listView);
        registerForContextMenu(listView);

        SharedPreferences sharedPreferences = this.getSharedPreferences("erpam.favouriteplaces",Context.MODE_PRIVATE);
        arraySize = sharedPreferences.getInt("arraySize:", 0);

        places = new ArrayList<String>();
        places.add("Add a new place");

        locations = new ArrayList<LatLng>();
        locations.add(new LatLng(0, 0));

        arrayAdapter = new ArrayAdapter(this, android.R.layout.simple_list_item_1, places);
        listView.setAdapter(arrayAdapter);


        for (int i = 0; i < arraySize; i++) {
            places.add(sharedPreferences.getString("places: " + Integer.toString((i+1)), ""));
            locations.add(new LatLng(sharedPreferences.getFloat("lat " + Integer.toString(i),0.0f),sharedPreferences.getFloat("lng " + Integer.toString(i),0.0f)));
            arrayAdapter.notifyDataSetChanged();
        }


        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                Intent i = new Intent(getApplicationContext(),MapsActivity.class);

                i.putExtra("Address",position);


                startActivity(i);
            }
        });

    }

    public void changeName(View view) {
        RelativeLayout layout = (RelativeLayout)findViewById(R.id.secondLay);
        EditText text = (EditText)findViewById(R.id.theText);
        String check = text.getText().toString();
        Log.i("CHECK: ", "is: " + check);


        if(check.startsWith(" ")) {
            Toast.makeText(getApplicationContext(), "Name cannot start with a space!", Toast.LENGTH_LONG).show();
        }else if(check.equals("")) {
            Toast.makeText(getApplicationContext(), "The name cannot be empty",Toast.LENGTH_LONG).show();
        }else if(current != 0 && check != null) {
            places.set(current,text.getText().toString());
            arrayAdapter.notifyDataSetChanged();
            text.setText("");
            layout.animate().alpha(0f).setDuration(400);
            layout.setVisibility(View.GONE);
        }



    }
}
