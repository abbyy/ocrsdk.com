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
		restClient.applicationId = args[0];
		restClient.password = args[1];
		
		String filePath = args[2];
		String outputFile = args[3];
		
		ProcessingSettings settings = new ProcessingSettings();
		
		try {
			Task task = null;
			if(outputFile.endsWith("xml")) {
				System.out.println( "Recognizing barcodes in image" );
				task = restClient.processBarcodeField(filePath, settings);				
			} else if(outputFile.endsWith("vcf")) {
				System.out.println( "Recognition of business card" );
				task = restClient.processBusinessCard(filePath, settings);
			} else {
				if(outputFile.endsWith("pdf")) {
					System.out.println( "Image will be converted to searchable pdf" );
					settings.setOutputFormat( ProcessingSettings.OutputFormat.pdfSearchable );
				} else if(outputFile.endsWith("txt")) {
					System.out.println( "Image will be converted to plain text" );
					settings.setOutputFormat( ProcessingSettings.OutputFormat.txt );
				}
				else {
					System.out.println( "Seems that the output format is unknown. Trying to convert the image to searchable pdf.");
				}			
				System.out.println( "Recognition with English language. If your documents contain other languages, please specify them." );
				System.out.println( "Uploading.." );
				task = restClient.processImage(filePath, settings);
			}
			
			while( task.IsTaskActive() ) {
				Thread.sleep(2000);
				
				System.out.println( "Waiting.." );
				task = restClient.getTaskStatus(task.Id);
			}
			
			if( task.Status == Task.TaskStatus.Completed ) {
				System.out.println( "Downloading.." );
				restClient.downloadResult(task, outputFile);
				System.out.println( "Ready" );
			} else if( task.Status == Task.TaskStatus.NotEnoughCredits ) {
				System.out.println( "Not enough credits to process document. Please add more pages to your application's account." );
			} else {
				System.out.println( "Task failed" );
			}
			
			
		} catch( Exception e) {
			System.out.println( "Exception occured:" + e.getMessage() );
		}
	}

}
