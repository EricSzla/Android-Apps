package com.example.eryk.aircar.tabs;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentStatePagerAdapter;
import android.support.v4.content.ContextCompat;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.style.ImageSpan;
import android.util.Log;

import com.example.eryk.aircar.R;
import com.example.eryk.aircar.fragments.FavouriteFragment;
import com.example.eryk.aircar.fragments.HistoryFragment;
import com.example.eryk.aircar.fragments.MapFragment;
import com.example.eryk.aircar.fragments.SearchFragment;

/**
 * Created by Eryk on 21/10/2016.
 * Class used for viewPager (main activity)
 * Handles the fragments
 */

public class MyPagerAdapter extends FragmentStatePagerAdapter{

    // Set the icons //android.R.drawable.ic_menu_recent_history
    private int icons[] = {R.drawable.ic_search_white_18dp, R.drawable.ic_favorite_border_white_18dp, R.drawable.ic_history_white_18dp, android.R.drawable.ic_dialog_map};
    private Context context; // context

    // Constructor
    public MyPagerAdapter(FragmentManager fm, Context context) {
        super(fm); // call super (fragmentstatepageradapter)
        this.context = context; // set context
    }


    // Overide getItem method
    @Override
    public Fragment getItem(int position) {
        Log.i("Pos: ", Integer.toString(position));
        switch (position) { // check position
            case 0:
                return SearchFragment.getInstance(position); // return SearchFragment
            case 1:
                return FavouriteFragment.getInstance(position); // return FavouriteFragment
            case 2:
                return HistoryFragment.getInstance(position); // return HistoryFragment
            case 3:
                return MapFragment.getInstance(position); // return MapFragment
            default:
                return SearchFragment.getInstance(position); // return SearchFragment
        }
    }

    // overide getCoutn to return length of icons array
    @Override
    public int getCount() {
        return icons.length;
    }

    // Draw icons
    @Override
    public CharSequence getPageTitle(int position) {
        Drawable drawable = ContextCompat.getDrawable(context, icons[position]);
        drawable.setBounds(0, 0, 120, 120);
        ImageSpan imageSpan = new ImageSpan(drawable);
        SpannableString spannableString = new SpannableString(" ");
        spannableString.setSpan(imageSpan, 0, spannableString.length(), Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
        return spannableString;
    }
}
