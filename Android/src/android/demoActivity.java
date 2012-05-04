package android;

import java.io.BufferedReader;
import java.io.FileReader;

import android.app.*;
import android.os.Bundle;
import android.widget.TextView;
import com.abbyy.ocrsdk.*;


public class demoActivity extends Activity {
	/** Called when the activity is first created. */
	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		tv = new TextView(this);
		tv.setText("Hello, cloud ocr\n");
		setContentView(tv);

		new Thread( new Worker() ).start();
	}

	TextView tv;


	class Worker implements Runnable {


		public void run() {
			try {
				Thread.sleep(1000);
				displayMessage( "Starting.." );
				Client restClient = new Client();
				restClient.ApplicationId = "<your app_id>";
				restClient.Password = "<your app_password>";
				
				String filePath = "/sdcard/00000001.jpg";
				String outputFile = "/sdcard/result.txt";
				
				ProcessingSettings settings = new ProcessingSettings();
				settings.setOutputFormat( ProcessingSettings.OutputFormat.txt );
				
				displayMessage( "Uploading.." );
				Task task = restClient.ProcessImage(filePath, settings);
				
				while( task.IsTaskActive() ) {
					Thread.sleep(2000);
					
					displayMessage( "Waiting.." );
					task = restClient.GetTaskStatus(task.Id);
				}
				
				if( task.Status == Task.TaskStatus.Completed ) {
					displayMessage( "Downloading.." );
					restClient.DownloadResult(task, outputFile);
				} else if( task.Status = Task.TaskStatus.NotEnoughCredits ) {
					displayMessage( "Not enough credits to process task. Add more pages to your application's account." );
				} else {
					displayMessage( "Task failed" );
				}
				
				displayMessage( "Ready" );

				
				StringBuffer contents = new StringBuffer(); 
				BufferedReader reader = new BufferedReader(new FileReader(outputFile)); 
				String text = null; 
				while ((text = reader.readLine()) != null) { 
					contents.append(text) 
					.append(System.getProperty( 
							"line.separator")); 
				}
				
				displayMessage( contents.toString() );
				
			} catch ( Exception e ) {
				displayMessage( "Error: " + e.getMessage() );
			}
		}

		private void displayMessage( String text )
		{
			tv.post( new MessagePoster( text ) );
		}

		class MessagePoster implements Runnable {
			public MessagePoster( String message )
			{
				_message = message;
			}

			public void run() {
				tv.append( _message + "\n" );
				setContentView( tv );
			}

			private final String _message;
		}
	}
}
