package erpam.pokequiz;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import java.util.ArrayList;

/**
 * Created by Eryk on 23/07/16.
 */
public class MenuAdapter extends ArrayAdapter<String> {

    Context context;
    ArrayList<String> titleArray;
    int[] ids;

    MenuAdapter(Context c, ArrayList<String> titles, int[] l){
        super(c,R.layout.activity_firstmenu,R.id.singleRowText,titles);
        this.context = c;
        this.titleArray = titles;
        this.ids = l;

    }

    class Holder{
        ImageView myImage;
        TextView name;

        Holder(View v){
            myImage = (ImageView) v.findViewById(R.id.singleRowImage);
            name = (TextView) v.findViewById(R.id.singleRowText);
        }
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        View row = convertView;
        Holder holder = null;

        if(row == null){
            LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            row = inflater.inflate(R.layout.firstmenu_singlerow,parent,false);
            holder = new Holder(row);
            row.setTag(holder);
        }else{
            holder = (Holder) row.getTag();
        }

        int a = ids[position];
        if(a != 0) {
            holder.myImage.setImageResource(ids[position]);
        }
        holder.name.setText(titleArray.get(position));
        if(position == 0 || position == 4 || position == 8){
            holder.myImage.setVisibility(View.GONE);
            holder.name.setTextAlignment(View.TEXT_ALIGNMENT_CENTER);
        }else{

        }
        return row;
    }


}
