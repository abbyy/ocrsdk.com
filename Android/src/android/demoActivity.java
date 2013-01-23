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

				- Please provide credentials and remove this line.
				
				// Name of application you created
				restClient.applicationId = "<your app_id>";
				// You should get e-mail from ABBYY Cloud OCR SDK service with the application password
				restClient.password = "<your app_password>";
				
				- Upload test image to sd card or modify sample to get image from camera. Remove this line afterwards.
				String filePath = "/sdcard/00000001.jpg";
				
				String outputFile = "/sdcard/result.txt";
				String language = "English"; // Comma-separated list: Japanese,English or German,French,Spanish etc.
				
				ProcessingSettings settings = new ProcessingSettings();
				settings.setOutputFormat( ProcessingSettings.OutputFormat.txt );
				settings.setLanguage(language);
				
				displayMessage( "Uploading.." );
				Task task = restClient.processImage(filePath, settings);
				
				// If you want to process business cards, uncomment this
				/*
				BusCardSettings busCardSettings = new BusCardSettings();
				busCardSettings.setLanguage(language);
				busCardSettings.setOutputFormat(BusCardSettings.OutputFormat.xml);
				Task task = restClient.processBusinessCard(filePath, busCardSettings);
				*/
				
				while( task.isTaskActive() ) {
					Thread.sleep(2000);
					
					displayMessage( "Waiting.." );
					task = restClient.getTaskStatus(task.Id);
				}
				
				if( task.Status == Task.TaskStatus.Completed ) {
					displayMessage( "Downloading.." );
					restClient.downloadResult(task, outputFile);
				} else if( task.Status == Task.TaskStatus.NotEnoughCredits ) {
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
