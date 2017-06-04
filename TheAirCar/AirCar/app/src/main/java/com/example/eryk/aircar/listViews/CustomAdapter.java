package com.example.eryk.aircar.listViews;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.ImageView;
import android.widget.TextView;

import com.example.eryk.aircar.R;

import java.util.ArrayList;

import static android.view.View.GONE;

/**
 * Created by Eryk on 21/11/2016.
 * CustomAdapter deals with setting up the 'wait msg'
 * e.g. history loading
 * The msg is displayed as an element of listview
 */

public class CustomAdapter extends ArrayAdapter<String> {

    public CustomAdapter(Context context, int resource, ArrayList<String> items) {
        super(context, resource, items);

    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {

        View v = convertView;
        if (v == null) {
            LayoutInflater vi;
            vi = LayoutInflater.from(getContext());
            v = vi.inflate(R.layout.advanced_list_row, null);
        }

        String p = getItem(position); // get string

        if (p != null) {
            // find views
            ImageView tt1 = (ImageView) v.findViewById(R.id.imageView);
            TextView tt2 = (TextView) v.findViewById(R.id.imageText);

            if (tt1 != null) {
                tt1.setImageResource(R.drawable.notfound); // set image
            }

            if (tt2 != null) {
                tt2.setText(p); // set text and change colors
                v.setBackgroundColor(v.getResources().getColor(R.color.main_blue));
                tt2.setTextColor(v.getResources().getColor(R.color.white));
            }

        }

        return v;
    }

}