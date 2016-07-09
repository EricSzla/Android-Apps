package erpam.weatherapp;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.EditText;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLEncoder;

public class MainActivity extends AppCompatActivity {


    EditText text;
    TextView weatheroutput;
    RelativeLayout downloadedImg = null;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        text = (EditText) findViewById(R.id.cityName);
        weatheroutput = (TextView) findViewById(R.id.output);

        downloadedImg = (RelativeLayout) findViewById(R.id.mainLayout);
        downloadImage(downloadedImg);
    }

    public void findWeather(View view){
        InputMethodManager mgr = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
        mgr.hideSoftInputFromWindow(text.getWindowToken(),0);

        Log.i("City name",text.getText().toString());
        try {

            weatheroutput.setAlpha(0f);
            String encodedCityName = URLEncoder.encode(text.getText().toString(), "UTF-8");
            DownloadTask task = new DownloadTask();
            task.execute("http://api.openweathermap.org/data/2.5/weather?q=" + encodedCityName + "&appid=21683d9217a6653a3b9d92111ecb61d4");
            weatheroutput.animate().alpha(1f).setDuration(1000);

        } catch (UnsupportedEncodingException e) {
            Toast.makeText(getApplicationContext(),"Could not find the weather", Toast.LENGTH_LONG).show();
        }

    }


    public class DownloadTask extends AsyncTask<String, Void, String> {

        @Override
        protected String doInBackground(String... urls) {

            String result = "";
            URL url = null;
            HttpURLConnection urlConnection = null;

            try {
                url = new URL(urls[0]);
                urlConnection = (HttpURLConnection) url.openConnection();
                InputStream in = urlConnection.getInputStream();
                InputStreamReader reader = new InputStreamReader(in);

                int data = reader.read();

                while (data != -1) {

                    char current = (char) data;
                    result += current;
                    data = reader.read();
                }
                return result;

            } catch (Exception e) {
                Toast.makeText(getApplicationContext(),"Could not find the weather", Toast.LENGTH_LONG).show();
            }
            return "Failed";
        }

        @Override
        protected void onPostExecute(String s) {
            super.onPostExecute(s);
            String weatherinfo = "";
            try {
                String message = "";
                JSONObject jsonObject = new JSONObject(s);
                weatherinfo = jsonObject.getString("weather");
                JSONArray arr = new JSONArray(weatherinfo);
                for(int i = 0; i < arr.length(); i++){
                    JSONObject jsonPart = arr.getJSONObject(i);

                    String main = "";
                    String description = "";
                    main = jsonPart.getString("main");
                    description = jsonPart.getString("description");

                    if(main != "" && description != ""){
                        message+= main + ": " + description + "\r\n";
                    }
                }

                if(message != ""){
                    weatheroutput.setText(message);
                }else{
                    weatheroutput.setText("");
                    Toast.makeText(getApplicationContext(),"Could not find the weather", Toast.LENGTH_LONG).show();
                }
            }catch (JSONException e) {
                weatheroutput.setText("");
                Toast.makeText(getApplicationContext(),"Could not find the weather", Toast.LENGTH_LONG).show();
            }
            Log.i("Website content", weatherinfo);
        }
    }

    public class ImageDownloader extends AsyncTask<String,Void,Bitmap>{

        @Override
        protected Bitmap doInBackground(String... urls) {

            try{
                URL url = new URL(urls[0]);
                HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                connection.connect();
                InputStream inputStream = connection.getInputStream();
                Bitmap myBitMap = BitmapFactory.decodeStream(inputStream);
                return myBitMap;

            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            } catch (Exception e) {
                e.printStackTrace();
            }

            return null;
        }
    }

    public void downloadImage(View view) {
        ImageDownloader task = new ImageDownloader();
        try {
            Bitmap myImage = task.execute("https://images.unsplash.com/photo-1434434319959-1f886517e1fe?ixlib=rb-0.3.5&q=80&fm=jpg&crop=entropy&s=218dfdd2c0735dbd6ca0f718064a748b").get();
            Drawable d = new BitmapDrawable(getResources(),myImage);
            downloadedImg.setBackground(d);
            Log.i("Done","Done");
        }catch (Exception e) {
            e.printStackTrace();
        }
    }
}
