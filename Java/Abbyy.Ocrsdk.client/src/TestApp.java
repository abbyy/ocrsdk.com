import com.abbyy.ocrsdk.*;

public class TestApp {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		System.out.println( "App started" );
		
		if( args.length != 4 ) {
			System.out.println( "Invalid arugments. Usage:" );
			System.out.println( "program <app id> <password> <input file> <output file>" );
			return;
		}
		
		Client restClient = new Client();
		restClient.ApplicationId = args[0];
		restClient.Password = args[1];
		
		String filePath = args[2];
		String outputFile = args[3];
		
		ProcessingSettings settings = new ProcessingSettings();
		
		try {
			System.out.println( "Uploading.." );
			Task task = restClient.ProcessImage(filePath, settings);
			
			while( task.IsTaskActive() ) {
				Thread.sleep(2000);
				
				System.out.println( "Waiting.." );
				task = restClient.GetTaskStatus(task.Id);
			}
			
			if( task.Status == Task.TaskStatus.Completed ) {
				System.out.println( "Downloading.." );
				restClient.DownloadResult(task, outputFile);
			} else {
				System.out.println( "Task failed" );
			}
			
			System.out.println( "Ready" );
			
		} catch( Exception e) {
			System.out.println( "Exception occured:" + e.getMessage() );
		}
	}

}
