package abbyy.ocrsdk.android;

import java.io.FileOutputStream;

import com.abbyy.ocrsdk.*;

import android.app.*;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.AsyncTask;

public class AsyncProcessTask extends AsyncTask<String, String, Boolean> {

	public AsyncProcessTask(ResultsActivity activity) {
		this.activity = activity;
		dialog = new ProgressDialog(activity);
	}

	private ProgressDialog dialog;
	/** application context. */
	private final ResultsActivity activity;

	protected void onPreExecute() {
		dialog.setMessage("Processing");
		dialog.setCancelable(false);
		dialog.setCanceledOnTouchOutside(false);
		dialog.show();
	}

	protected void onPostExecute(Boolean result) {
		if (dialog.isShowing()) {
			dialog.dismiss();
		}
		
		activity.updateResults();
	}

	@Override
	protected Boolean doInBackground(String... args) {

		String inputFile = args[0];
		String outputFile = args[1];

		try {
			Client restClient = new Client();
			
			!!! Please provide credentials and remove this line. !!!
			
			// Name of application you created
			restClient.applicationId = "<your app_id>";
			// You should get e-mail from ABBYY Cloud OCR SDK service with the application password
			restClient.password = "<your app_password>";
			
			// Obtain installation id when running the application for the first time
			SharedPreferences settings = activity.getPreferences(Activity.MODE_PRIVATE);
			String instIdName = "installationId";
			if( !settings.contains(instIdName)) {
				// Get installation id from server using device id
				String deviceId = android.provider.Settings.Secure.getString(activity.getContentResolver(), 
						android.provider.Settings.Secure.ANDROID_ID);
				
				// Obtain installation id from server
				publishProgress( "First run: obtaining installation id..");
				String installationId = restClient.activateNewInstallation(deviceId);
				publishProgress( "Done. Installation id is '" + installationId + "'");
				
				SharedPreferences.Editor editor = settings.edit();
				editor.putString(instIdName, installationId);
				editor.commit();
			} 
			
			String installationId = settings.getString(instIdName, "");
			restClient.applicationId += installationId;
			
			publishProgress( "Uploading image...");
			
			String language = "English"; // Comma-separated list: Japanese,English or German,French,Spanish etc.
			
			ProcessingSettings processingSettings = new ProcessingSettings();
			processingSettings.setOutputFormat( ProcessingSettings.OutputFormat.txt );
			processingSettings.setLanguage(language);
			
			publishProgress("Uploading..");

			// If you want to process business cards, uncomment this
			/*
			BusCardSettings busCardSettings = new BusCardSettings();
			busCardSettings.setLanguage(language);
			busCardSettings.setOutputFormat(BusCardSettings.OutputFormat.xml);
			Task task = restClient.processBusinessCard(filePath, busCardSettings);
			*/
			Task task = restClient.processImage(inputFile, processingSettings);
			
			while( task.isTaskActive() ) {
				Thread.sleep(2000);
				
				publishProgress( "Waiting.." );
				task = restClient.getTaskStatus(task.Id);
			}
			
			if( task.Status == Task.TaskStatus.Completed ) {
				publishProgress( "Downloading.." );
				FileOutputStream fos = activity.openFileOutput(outputFile,Context.MODE_PRIVATE);
				
				restClient.downloadResult(task, fos);
				
				fos.close();
				
				publishProgress( "Ready" );
			} else if( task.Status == Task.TaskStatus.NotEnoughCredits ) {
				publishProgress( "Not enough credits to process task. Add more pages to your application's account." );
			} else {
				publishProgress( "Task failed" );
			}
			
			return true;
		} catch (Exception e) {
			publishProgress( "Error: " + e.getMessage());
			return false;
		}
	}

	@Override
	protected void onProgressUpdate(String... values) {
		// TODO Auto-generated method stub
		String stage = values[0];
		dialog.setMessage(stage);
		// dialog.setProgress(values[0]);
	}

}
