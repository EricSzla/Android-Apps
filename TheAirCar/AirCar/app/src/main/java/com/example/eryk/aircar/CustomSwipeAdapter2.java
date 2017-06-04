package com.example.eryk.aircar;

import android.content.Context;
import android.net.Uri;
import android.support.v4.view.PagerAdapter;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;

import java.net.URI;

/**
 * Created by Eryk on 23/11/16.
 * Deals with ViewPager (in Car.class)
 * Displays images of the car fetched from database.
 */

public class CustomSwipeAdapter2 extends PagerAdapter {

    // Variables
    private Uri[] images_URI;
    private Context context;
    private LayoutInflater layoutInflater;

    // Constructor
    public CustomSwipeAdapter2(Context ctx, Uri[] uris){
        this.context = ctx;
        this.images_URI = uris;

    }

    // overide getCount
    @Override
    public int getCount() {
        return images_URI.length;
    }

    @Override
    public boolean isViewFromObject(View view, Object object) {
        return (view==(LinearLayout)object);
    }

    @Override
    public Object instantiateItem(ViewGroup container, int position) {
        layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View item_view = layoutInflater.inflate(R.layout.swipe_layout,container,false);
        ImageView imageView = (ImageView) item_view.findViewById(R.id.swipe_layout_swipe_id); // find imageView
        glide(images_URI[position],imageView); // glide
        container.addView(item_view);

        return item_view;
    }

    @Override
    public void destroyItem(ViewGroup container, int position, Object object) {
        container.removeView((LinearLayout) object);
    }

    public void glide(Uri uri, ImageView imageView) {
        Glide.with(context)
                .load(uri)
                .diskCacheStrategy(DiskCacheStrategy.RESULT)
                //.skipMemoryCache(true)
                .fitCenter()
                //.override(150,150)
                .into(imageView);
    }
}
