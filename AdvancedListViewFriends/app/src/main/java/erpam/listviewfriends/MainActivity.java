package erpam.listviewfriends;

import android.content.Context;
import android.content.res.Resources;
import android.media.Image;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;

import org.w3c.dom.Text;

import java.util.List;

public class MainActivity extends AppCompatActivity {

    String[] titles;
    String[] desc;
    int[] images = {R.drawable.instagram1,R.drawable.instagram2,R.drawable.instagram3,R.drawable.instagram1,R.drawable.instagram2,R.drawable.instagram3,R.drawable.instagram1,R.drawable.instagram2,R.drawable.instagram3,R.drawable.instagram1,R.drawable.instagram2,R.drawable.instagram3,R.drawable.instagram1};

    ListView l;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Resources resources = getResources();
        titles = resources.getStringArray(R.array.titles);
        desc = resources.getStringArray(R.array.description);

        l = (ListView) findViewById(R.id.listView);

        Adapter adapter = new Adapter(this,titles,images,desc);
        l.setAdapter(adapter);


    }
}

class Adapter extends ArrayAdapter<String>{

    Context context;
    int[] images;
    String[] titleArray;
    String[] desc;

    Adapter(Context c, String titles[], int images[], String desc[]){
        super(c,R.layout.single_row,R.id.textView,titles);
        this.context = c;
        this.images = images;
        this.titleArray = titles;
        this.desc = desc;
    }

    class ViewHolder{
        ImageView myImage;
        TextView titles;
        TextView desc;

        ViewHolder(View v){
            myImage = (ImageView) v.findViewById(R.id.imageView);
            titles = (TextView) v.findViewById(R.id.textView);
            desc = (TextView) v.findViewById(R.id.textView2);
        }
    }
    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        View row = convertView;
        ViewHolder holder = null;

        if(row ==null){
            LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            row = inflater.inflate(R.layout.single_row,parent,false);
            holder = new ViewHolder(row);
            row.setTag(holder);
        }else{
            holder = (ViewHolder) row.getTag();
        }

        holder.myImage.setImageResource(images[position]);
        //imageView.setImageURI();
        holder.titles.setText(titleArray[position]);
        holder.desc.setText(desc[position]);

        return row;
    }
}
