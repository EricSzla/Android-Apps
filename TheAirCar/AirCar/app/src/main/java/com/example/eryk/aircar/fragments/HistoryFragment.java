package com.example.eryk.aircar.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListView;

import com.example.eryk.aircar.Car;
import com.example.eryk.aircar.MainActivity;
import com.example.eryk.aircar.R;
import com.example.eryk.aircar.listViews.CustomAdapter;
import com.example.eryk.aircar.listViews.CustomAdapter3;
import com.parse.FindCallback;
import com.parse.ParseException;
import com.parse.ParseFile;
import com.parse.ParseObject;
import com.parse.ParseQuery;
import com.parse.ParseUser;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Created by Eryk on 20/11/2016.
 * Method deals with history fragment
 */

public class HistoryFragment extends android.support.v4.app.Fragment {
    private static String NO_DATA_FOUND;                // no data found string
    private ListView listView;                          // listview
    private List<HashMap<String,String>> historyList;   // stores history collection data
    private List<HashMap<String,String>> car_postList;  // stores carPost collection data

    // get instance is accessed from mypageradapter
    public static HistoryFragment getInstance(int position) {
        HistoryFragment historyFragment = new HistoryFragment();
        return historyFragment;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.layout_listview, container, false);

        NO_DATA_FOUND = rootView.getResources().getString(R.string.noHistoryFound); // set string
        fetchHistory(); // fetch history posts

        // in the meantime set the list view to "loading history, please wait..."
        String[] countries = new String[] {rootView.getResources().getString(R.string.loadingHistory)};
        final ArrayList<String> list  = new ArrayList<>();
        for(int i =0; i < countries.length; i++){
            list.add(i,countries[i]);
        }
        CustomAdapter adapter = new CustomAdapter(getActivity(),R.layout.advanced_list_row,list);
        listView = (ListView) rootView.findViewById(R.id.layout_favourite_listView);
        listView.setAdapter(adapter);
        return rootView;
    }

    private void fetchHistory(){
        ParseQuery<ParseObject> historyQuery = new ParseQuery<>(MainActivity.TAGS.HISTORY_TAG);              // create query history collection
        historyQuery.whereEqualTo(MainActivity.TAGS.USERID_TAG, ParseUser.getCurrentUser().getObjectId());   // where userid = current used
        historyQuery.findInBackground(new FindCallback<ParseObject>() {
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // find in background
                if(e == null){ // check for errors
                    if(objects.size() > 0){      // check if data is found
                        //History posts have been found.
                        if(historyList == null){
                            historyList = new ArrayList<>(); // initialize history list
                        }

                        // put data in history list
                        for(int i = 0; i < objects.size();i++){
                            //Store data from history table
                            HashMap<String,String> hashMap = new HashMap<String, String>(); // local hashmap to store fetched data (for each object)
                            hashMap.put(MainActivity.TAGS.POSTID_TAG,objects.get(i).get(MainActivity.TAGS.POSTID_TAG).toString()); // put postid
                            hashMap.put(MainActivity.TAGS.RENTFROM_TAG,objects.get(i).get(MainActivity.TAGS.RENTFROM_TAG).toString()); // put rentfrom
                            hashMap.put(MainActivity.TAGS.RENTTO_TAG,objects.get(i).get(MainActivity.TAGS.RENTTO_TAG).toString()); // put rent to
                            historyList.add(i,hashMap); // add to list
                        }

                        // fetch post from carPost collection for each object fetched from history collection
                        for(int i = 0; i < historyList.size();i++) {
                            fetchPosts(historyList.get(i).get(MainActivity.TAGS.POSTID_TAG),i);
                        }
                    }else{
                        setNoDataFound();
                    }
                }else{
                    setNoDataFound();
                }
            }
        });
    }// end of fetch History

    private void setNoDataFound(){
        //There was no posts found, set the string to NO_DATA_FOUND
        //And update the adapter
        //This will display appropriate msg to the user.
        String[] countries = new String[] {NO_DATA_FOUND};
        final ArrayList<String> list  = new ArrayList<>();
        for(int i =0; i < countries.length; i++){
            list.add(i,countries[i]);
        }

        CustomAdapter adapter = new CustomAdapter(MainActivity.getActivityThis(),R.layout.advanced_list_row,list);
        listView.setAdapter(adapter);
    }

    private void fetchPosts(final String postId, final int i){
        ParseQuery<ParseObject> carPost = new ParseQuery<>(MainActivity.TAGS.CARPOST_TAG);      // query carpost
        carPost.whereEqualTo(MainActivity.TAGS.OBJECTID_TAG,postId);                            // where objectId = postid (passed in parameter)
        carPost.findInBackground(new FindCallback<ParseObject>() {
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // find
                if(e == null){ // chech exceptions
                    if(objects.size() > 0){ // check if data has been found
                        // Car posts found, store them in temporary hashmap which will be added to the main ArrayList
                        HashMap<String,String> history_posts = new HashMap<>();
                        history_posts.put(MainActivity.TAGS.POSTID_TAG,objects.get(0).getObjectId()); // put post id in hashmap
                        history_posts.put(MainActivity.TAGS.DESCRIPTION_TAG,objects.get(0).get(MainActivity.TAGS.DESCRIPTION_TAG).toString()); // put description

                        ParseFile postImage = objects.get(0).getParseFile(MainActivity.TAGS.CAR_PICTURE); // Get image as ParseFile
                        String imageUrl = postImage.getUrl(); // Get the image URL
                        history_posts.put(MainActivity.TAGS.URL_TAG,imageUrl); // Store it to the temp HashMap

                        // Prevent null pointer exception
                        if(car_postList == null || car_postList.size() >= historyList.size()){
                            car_postList = new ArrayList<>(); // initialize carpost list
                        }

                        if(i <= car_postList.size()) { // prevent null pointer exception
                            car_postList.add(i, history_posts); // Add the hashmap to the list
                            //region Logs
                            Log.i("HISTORY_POSTS: ", history_posts.toString());
                            Log.i("postsSize: ", String.valueOf(car_postList.size()));
                            Log.i("posts_id: ", String.valueOf(historyList.size()));
                            //endregion
                            if (car_postList.size() <= historyList.size()) { // Making sure all posts are fetched by comparing the sizes
                                Log.i("FRAGMENT", "READY");
                                // Updating the adapter of the listview
                                CustomAdapter3 adapter = new CustomAdapter3(MainActivity.getActivityThis(),car_postList); // new CustomAdapter3
                                if (listView != null) { // check listview
                                    listView.setAdapter(adapter); // set adapter
                                    listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                                        @Override
                                        public void onItemClick(AdapterView<?> parent, View view, int position, long id) { // set on click listener
                                            Intent i = new Intent(getContext(),Car.class); //start car.class
                                            i.putExtra(MainActivity.TAGS.SHOW_RESERVE_TAG,false); // put boolean
                                            i.putExtra(MainActivity.TAGS.POSTID_TAG,car_postList.get(position).get(MainActivity.TAGS.POSTID_TAG)); //put postid
                                            startActivity(i);//start activity
                                        }
                                    });
                                }
                            }
                        }else{
                            fetchPosts(postId, i); // try to fetch again and see if the size of car_postList was increased
                        }
                    }
                }
            }
        });
    }
}
