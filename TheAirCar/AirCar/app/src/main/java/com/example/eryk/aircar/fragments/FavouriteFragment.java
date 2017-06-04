package com.example.eryk.aircar.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.Toast;

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

import static com.example.eryk.aircar.MainActivity.getActivityThis;

/**
 * Created by Eryk on 20/11/2016.
 * Fragment that deals with the favourite cars, composed of a listView.
 */

public class FavouriteFragment extends Fragment {
    private String[] fav_posts_id;      // String to store post's ids
    private static List<HashMap<String,String>> fav_posts_list; // List that store hashmaps of cars
    private ListView listView; // list view to display data

    // getInstance is accessed from MyPagerAdapter
    public static FavouriteFragment getInstance(int position) {
        return new FavouriteFragment();
    }

    // overide onCreateView
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        final View rootView = inflater.inflate(R.layout.layout_listview, container, false); // inflate rootview
        fetch_fav_ids();  // call fetch_fav_ids();
        String[] posts = new String[]{rootView.getResources().getString(R.string.loadingFavPosts)}; // Display wait masg
        final ArrayList<String> list = new ArrayList<>(); // create a arraylist to store the posts[] string
        for (int i = 0; i < posts.length; i++) {
            list.add(i, posts[i]); // add it
        }

        CustomAdapter adapter = new CustomAdapter(getContext(), R.layout.advanced_list_row, list); // create new customadapter and pass list
        listView = (ListView) rootView.findViewById(R.id.layout_favourite_listView); // find listview
        if (listView != null) {
            listView.setAdapter(adapter); // set adapter
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    Toast.makeText(getActivityThis(), rootView.getResources().getString(R.string.checkNetwork), Toast.LENGTH_SHORT).show();
                    // inform user on click
                }
            });
        }

        return rootView;
    }


    /*
        Method that will display error msg to the user if there is no posts added to favouites
     */
    private void setNoPostFound(){
        String[] posts = new String[]{getResources().getString(R.string.noPostFound)};
        final ArrayList<String> list = new ArrayList<>();
        for (int i = 0; i < posts.length; i++) {
            list.add(i, posts[i]);
        }

        CustomAdapter adapter = new CustomAdapter(getActivityThis(), R.layout.advanced_list_row, list);
        if (listView != null) {
            listView.setAdapter(adapter);
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    Toast.makeText(getActivityThis(), getResources().getString(R.string.checkNetwork), Toast.LENGTH_SHORT).show();
                }
            });
        }
    }

    /*
        Method will fetch the ids from FAVOURITE collection
     */
    private void fetch_fav_ids(){
         // Create parse object that will store the fetched data
        ParseQuery<ParseObject> fav_posts_query = new ParseQuery<ParseObject>(MainActivity.TAGS.FAVOURITE);
        ParseUser user = ParseUser.getCurrentUser(); // Get the current user
        fav_posts_query.whereEqualTo(MainActivity.TAGS.USERID_TAG,user.getObjectId());  // where  userID = current user id
        fav_posts_query.findInBackground(new FindCallback<ParseObject>() { // Execute query
            @Override
            public void done(List<ParseObject> objects, ParseException e) { // find in background
                if(e == null){ // Check if there was no errors
                    if(objects.size() > 0){ // if post were found
                        Log.i("Favourite", "Found posts");
                        fav_posts_id = new String[objects.size()]; // initialize string array
                        for(int i =0; i < objects.size();i++){ // Iterate through all objects to find IDS
                            fav_posts_id[i] = objects.get(i).get(MainActivity.TAGS.POSTID_TAG).toString(); // add it to the string array
                        }

                        // Fetch data from post table
                        for(int i =0; i < fav_posts_id.length;i++) {
                            findPosts(fav_posts_id[i], i);
                        }

                    }else{
                        setNoPostFound();
                        Log.i("Favourite", "Posts not found");
                    }
                }else{
                    setNoPostFound();
                    Log.i("Favourite", "Error fetching posts");
                }
            }
        });
    }


    private void findPosts(final String fav_id, final int i){
        ParseQuery<ParseObject> posts_query = new ParseQuery<>(MainActivity.TAGS.CAR_POST);  // create query
        posts_query.whereEqualTo(MainActivity.TAGS.OBJECTID_TAG,fav_id);                     // where objectId == fav_id
        posts_query.findInBackground(new FindCallback<ParseObject>() {
            @Override
            public void done(List<ParseObject> objects, ParseException e) {   // find in background
                if(e == null && objects.size() > 0){  // check for exception and size
                    HashMap<String,String> fav_posts = new HashMap<>(); // new localHashmap
                    fav_posts.put(MainActivity.TAGS.POSTID_TAG,objects.get(0).getObjectId());   // put postsID in hashmap
                    fav_posts.put(MainActivity.TAGS.DESCRIPTION,objects.get(0).get(MainActivity.TAGS.DESCRIPTION).toString()); // put description in Hashmap

                    // Get image as ParseFile
                    ParseFile postImage = objects.get(0).getParseFile(MainActivity.TAGS.CAR_PICTURE);
                    String imageUrl = postImage.getUrl(); // Get the image URL
                    fav_posts.put(MainActivity.TAGS.URL_TAG,imageUrl); // Store it to the temp HashMap

                    if(fav_posts_list == null || fav_posts_list.size() >= fav_posts_id.length){
                        fav_posts_list = new ArrayList<>(); // initialize list
                    }

                    if(i <= fav_posts_list.size()) { // prevent null pointer exception
                        fav_posts_list.add(i, fav_posts); // add hashmap to the list

                        //region logs
                        Log.i("FAV_POSTS: ", fav_posts.toString());
                        Log.i("postsSize: ", String.valueOf(fav_posts_list.size()));
                        Log.i("posts_id: ", String.valueOf(fav_posts_id.length));
                        // endregion
                        if (fav_posts_list.size() <= fav_posts_id.length) { // Making sure all posts are fetched by comparing the sizes
                            Log.i("FRAGMENT", "READY");
                            // Updating the adapter of the listview
                            CustomAdapter3 adapter = new CustomAdapter3(MainActivity.getActivityThis(),fav_posts_list);  // Create new CustomAdapter3
                            if (listView != null) { // check if listView is null
                                listView.setAdapter(adapter);  // set adapater to the listview
                                listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                                    @Override
                                    public void onItemClick(AdapterView<?> parent, View view, int position, long id) { // set on click listener
                                        Intent i = new Intent(getContext(),Car.class); // new car intent
                                        i.putExtra(MainActivity.TAGS.SHOW_RESERVE_TAG,false); // add reserve boolean
                                        i.putExtra(MainActivity.TAGS.POSTID_TAG,fav_posts_list.get(position).get(MainActivity.TAGS.POSTID_TAG)); // add postid
                                        startActivity(i); // start activity
                                    }
                                });
                                registerForContextMenu(listView);  // This allows the longclick on listsView (to choose "View" and "Delete"), declared in MainActivity
                            }
                        }
                    }else{
                        findPosts(fav_id, i); // means background thread was not synchronised, try again (going to try again until 'i' meets if statement criteria
                    }
                }else{
                    setNoPostFound(); // call method
                    if(e != null){
                        Log.i("E: ", e.getMessage()); // print msg
                    }
                }
            }
        });
    }

    // getter
    public static String getId(int position){
        return fav_posts_list.get(position).get(MainActivity.TAGS.POSTID_TAG);
    }
}
