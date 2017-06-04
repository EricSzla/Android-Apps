package com.example.eryk.aircar;

import android.content.Context;
import android.support.v4.view.PagerAdapter;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;

/**
 * Created by Eryk on 23/11/16.
 *  This class is used for the ViewPager (in login activity).
 *  It is responsible for inflating the right image and handle the 'swipe' input.
 */

public class CustomSwipeAdapter extends PagerAdapter {

    private int[] image_res = {R.drawable.one, R.drawable.two, R.drawable.three};  // Initialize images to be displayed.
    private Context context;    // Context passed through constructor

    // Constuctor
    public CustomSwipeAdapter(Context ctx){
        this.context = ctx;
    }


    @Override
    public int getCount() {
        return image_res.length;
    } // Will return the lengh of an int array (images)

    @Override
    public boolean isViewFromObject(View view, Object object) {
        return (view==(LinearLayout)object);
    }

    @Override
    public Object instantiateItem(ViewGroup container, int position) {
        LayoutInflater layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);   // Get inflater
        View item_view = layoutInflater.inflate(R.layout.swipe_layout,container,false);                               // Inflate view
        ImageView imageView = (ImageView) item_view.findViewById(R.id.swipe_layout_swipe_id);                         // Find imageView

        if(imageView != null) {                                 // Check if imageView was found
            imageView.setImageResource(image_res[position]);    // If so then set the imageResource to the appropriate image
        }

        container.addView(item_view);                           // Add view to the container.

        return item_view;                                       // Return the view
    }

    @Override
    public void destroyItem(ViewGroup container, int position, Object object) {
        container.removeView((LinearLayout) object);
    }
}
