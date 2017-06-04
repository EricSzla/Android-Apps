package com.example.eryk.aircar.listViews;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.example.eryk.aircar.R;

import java.util.HashMap;
import java.util.List;

/**
 * Created by Eryk on 21/11/16.
 * Deals with setting up the listviews e.g. history list view or favourite list view
 */
public class CustomAdapter3 extends BaseAdapter {

    private Context context;
    private static final String URL_TAG = "url";
    private static final String DESCRIPTION_TAG = "description";
    private List<HashMap<String,String>> data; // 'car' data

    // constructor
    public CustomAdapter3(Context c, List<HashMap<String,String>> list){
        this.context = c;
        this.data = list;
    }

    //Optimisation, loads faster by 175 %
    class Holder{
        ImageView myImage;
        TextView tt2;

        Holder(View v){
            // find views
            myImage = (ImageView) v.findViewById(R.id.imageView);
            tt2 = (TextView) v.findViewById(R.id.imageText);
        }
    }

    @Override
    public int getCount() {
        return data.size();
    }

    @Override
    public Object getItem(int position) {
        return data.get(position);
    }

    @Override
    public long getItemId(int position) {
        return position;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        View row = convertView;
        Holder holder = null;

        if(row == null){ // check if row is null
            // if so then inflate and initialize new holder
            LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            row = inflater.inflate(R.layout.advanced_list_row,parent,false);
            holder = new Holder(row);
            row.setTag(holder); // set row tag
        }else{
            holder = (Holder) row.getTag(); // otherwise just get the tag
        }

        if(data != null) {
            glide(data.get(position).get(URL_TAG), holder.myImage); // glide the image
            if(data.get(position).get(DESCRIPTION_TAG).length() > 200){
                data.get(position).put(DESCRIPTION_TAG,data.get(position).get(DESCRIPTION_TAG).substring(0,200));
            }
            holder.tt2.setText(data.get(position).get(DESCRIPTION_TAG)); // set text
        }else{
            Log.i("Adapter3","NULL");
            holder.myImage.setImageResource(R.drawable.notfound); // set not found
            holder.tt2.setText(R.string.noPostFound); // set error text
        }
        return row;
    }


    // Method that takes picture URL and the imageView, and then downloads the picture and puts the picture in that ImageView (in the background)
    private void glide(String url, ImageView imageView) {
        Log.i("Adapter: ", "Gliding");
        Glide.with(context)
                .load(url)
                .placeholder(R.drawable.ic_media_play)
                .override(150,120)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .fitCenter()
                .into(imageView);
    }

}
