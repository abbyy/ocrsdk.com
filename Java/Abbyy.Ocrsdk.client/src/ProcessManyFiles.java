import com.abbyy.ocrsdk.*;
import java.util.*;
import java.io.*;

public class ProcessManyFiles {
	public static void main(String[] args) {

		System.out.println("Process multiple documents using ABBYY Cloud OCR SDK.\n");

		if (!checkAppId()) {
			return;
		}

		if (args.length < 2 ) {
			displayHelp();
			return;
		}

		ClientSettings.setupProxy();

		restClient = new Client();
		// replace with 'https://cloud-eu.ocrsdk.com' to enable secure connection
		// replace with 'https://cloud-westus.ocrsdk.com' if your application is created in US location
		restClient.serverUrl = "http://cloud-eu.ocrsdk.com";
		restClient.applicationId = ClientSettings.APPLICATION_ID;
		restClient.password = ClientSettings.PASSWORD;

		Vector<String> argList = new Vector<String>(Arrays.asList(args));

		// Select processing mode
		String mode = args[0];
		argList.remove(0);

		try {
			if (mode.equalsIgnoreCase("help")) {
				displayDetailedHelp(args[1]);
			} else if (mode.equalsIgnoreCase("recognize")) {
				performRecognition(argList);
			} else if (mode.equalsIgnoreCase("remote")) {
				performRemoteFileRecognition(argList);
			} else if (mode.equalsIgnoreCase("receipt")) {
				performReceiptRecognition(argList);
			}
		} catch (Exception e) {
			System.out.println("Exception occured: " + e.getMessage());
			e.printStackTrace();
		}

	}

	 /**
	 * Check that user specified application id and password.
	 * 
	 * @return false if no application id or password
	 */
	private static boolean checkAppId() {
		String appId = ClientSettings.APPLICATION_ID;
		String password = ClientSettings.PASSWORD;
		if (appId.isEmpty() || password.isEmpty()) {
			System.out
					.println("Error: No application id and password are specified.");
			System.out.println("Please specify them in ClientSettings.java.");
			return false;
		}
		return true;
	}

	private static void displayHelp() {
		System.out.println(
			"Recognize multiple files at once.\n" +
			"Usage:\n" +
			"  1. Recognize all files from a directory:\n" +
			"    java ProcessManyFiles recognize <imagesDir> <resultDir>\n" +
			"  2. Recognize files from url (experimental):\n" +
			"    java ProcessManyFiles remote <imageUrl> <resultDir>\n" +
			"  3. Recognize many files from urls in a file (experimental):\n" +
			"    java ProcessManyFiles remote <urlFilePath> <resultDir>\n" +
			"  4. Recognize all receipts from a directory:\n" +
			"    java ProcessManyFiles receipt <receiptsDir> <resultDir>\n" +
			"\n" +
			"For detailed help, call\n" +
			"  java ProcessManyFiles help <mode>\n" +
			"where <mode> is one of: recognize, remote, receipt"
		);
	}

	private static void displayDetailedHelp(String mode) {
		if (mode.equalsIgnoreCase("recognize")) {
			displayRecognizeHelp();
		} else if (mode.equalsIgnoreCase("remote")) {
			displayRemoteHelp();
		} else if (mode.equalsIgnoreCase("receipt")) {
			displayReceiptHelp();
		}
	}

	
	private static void displayRecognizeHelp() {
		System.out.println(
			"Recognize all images from a directory.\n"
			+ "\n"
			+ "Usage:\n"
			+ "  java ProcessManyFiles recognize [--lang=<languages>] [--format=<format>] <directory> <output dir>\n"
			+ "\n"
			+ "Possible output formats:\n"
			+ "  txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml\n"
			+ "  Default format is txt\n"
			+ "\n"
			+ "Examples:\n"
			+ "java ProcessManyFiles recognize ~/myImages ~/text\n"
			+ "java ProcessManyFiles recognize --lang=French,Spanish --format=pdfSearchable myImages ocrPdfImages\n" );

	}

	private static void performRecognition(Vector<String> argList)
		throws Exception {
		
		ProcessingSettings settings = new ProcessingSettings();
		settings.setLanguage( CmdLineOptions.extractRecognitionLanguage(argList) );
		settings.setOutputFormat( CmdLineOptions.extractOutputFormat(argList) );

		String sourceDirPath = argList.get(0);
		String targetDirPath = argList.get(1);
		setOutputPath( targetDirPath );

		File sourceDir = new File(sourceDirPath);

		File[] listOfFiles = sourceDir.listFiles();

		Vector<String> filesToProcess = new Vector<String>();

		for (int i = 0; i < listOfFiles.length; i++) {
			File file = listOfFiles[i];
			if (file.isFile()) {
				String fullPath = file.getAbsolutePath();
				filesToProcess.add(fullPath);
			} 		
		}

		Map<String,String> taskIds = submitAllFiles(filesToProcess, settings);

		waitAndDownloadResults( taskIds );
	}

	private static void displayBetaWarning() {
		System.out.println(
			"*** WARNING! You are using API that is in beta stage. ***\n"
			+ "*** It can change any time without notice or even be removed from ABBYY Cloud OCR SDK service. ***\n\n"
			);
	}

	private static void displayRemoteHelp() {
		displayBetaWarning();
		System.out.println(
			"Recognize images specified by URL.\n"
			+ "\n" 
			+ "Usage:\n" 
			+ "  java ProcessManyFiles remote [--lang=<languages>] [--format=<format>] <url|file with urls> <output dir>\n"
			+ "\n"
			+ "If url is specified then only one image from that url is recognized.\n"
			+ "If file is specified then all urls from that file are recognized as different tasks.\n"
			+ "\n"
			+ "Possible output formats:\n"
			+ "  txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml\n"
			+ "  Default format is txt\n"
			+ "\n"
			+ "Examples:\n"
			+ "java ProcessManyFiles remote https://github.com/abbyysdk/ocrsdk.com/blob/master/SampleData/Page_08.tif?raw=true ~/text\n"
			+ "java ProcessManyFiles remote --lang=French,Spanish --format=pdfSearchable ~/myUrlList.txt ocrPdfImages\n" );

	}

	private static void performRemoteFileRecognition( Vector<String> argList )
		throws Exception {
		displayBetaWarning();

		ProcessingSettings settings = new ProcessingSettings();
		settings.setLanguage( CmdLineOptions.extractRecognitionLanguage(argList) );
		settings.setOutputFormat( CmdLineOptions.extractOutputFormat(argList) );

		String remoteFile = argList.get(0);
		String targetDirPath = argList.get(1);
		setOutputPath( targetDirPath );

		Vector<String> urlsToProcess = new Vector<String>();
		if( remoteFile.startsWith( "http://" ) || remoteFile.startsWith( "https://" ) ) {
			urlsToProcess.add(remoteFile);
		} else {
			// Get url list from remoteFile
			BufferedReader br = new BufferedReader( new FileReader( remoteFile ) );
			try {
				String line;
				while( (line = br.readLine()) != null ) {
					urlsToProcess.add( line );
				}
			} finally {
				br.close();
			}
		}

		Map<String,String> taskIds = submitRemoteUrls(urlsToProcess, settings);
		waitAndDownloadResults( taskIds );
	}

	private static void displayReceiptHelp() {
		System.out.println(
			"Recognize all receipts from a directory.\n"
			+ "\n"
			+ "Usage:\n"
			+ "  java ProcessManyFiles receipt [--country=<countryNames>] <directory> <output dir>\n"
			+ "\n"
			+ "--country \n"
			+ "  Set the country where receipt was printed. You can set any country listed at"
			+ " https://ocrsdk.com/documentation/apireference/processReceipt/ or set comma-separated combination of them.\n"
			+ "  Default country is Usa\n"
			+ "\n"
			+ "Examples:\n"
			+ "java ProcessManyFiles receipt --country=Usa,Spain ~/myReceipts ~/text\n");
	}

	private static void performReceiptRecognition(Vector<String> argList)
		throws Exception {
		
		ReceiptSettings settings = new ReceiptSettings();
		settings.setReceiptCountry( CmdLineOptions.extractReceiptCountry(argList) );
	
		if (argList.size() < 2 ) {
			displayReceiptHelp();
			return;
		}

		String sourceDirPath = argList.get(0);
		String targetDirPath = argList.get(1);
		setOutputPath( targetDirPath );

		File sourceDir = new File(sourceDirPath);

		File[] listOfFiles = sourceDir.listFiles();

		Vector<String> filesToProcess = new Vector<String>();

		for (int i = 0; i < listOfFiles.length; i++) {
			File file = listOfFiles[i];
			if (file.isFile()) {
				String fullPath = file.getAbsolutePath();
				filesToProcess.add(fullPath);
			} 		
		}

		Map<String,String> taskIds = submitAllReceipts(filesToProcess, settings);

		waitAndDownloadResults( taskIds );
	}

	/**
	* Submit all files for recognition
	*
	* @return map task id, file name for submitted tasks
	*/
	private static Map<String,String> submitAllFiles(Vector<String> fileList, ProcessingSettings settings) throws Exception {
		System.out.println( String.format( "Uploading %d files..", fileList.size() ));

		Map<String,String> taskIds = new HashMap<String, String>();

		for (int fileIndex = 0; fileIndex < fileList.size(); fileIndex++ ) {
			String filePath = fileList.get(fileIndex);

			File file = new File(filePath);
			String fileBase = file.getName();
			if (fileBase.indexOf(".") > 0) {
				fileBase = fileBase.substring(0, fileBase.lastIndexOf("."));
			}

			System.out.println( filePath );
			Task task = restClient.processImage( filePath, settings );
			taskIds.put(task.Id, fileBase + settings.getOutputFileExt());	
		}
		return taskIds;
	}

	private static Map<String,String> submitAllReceipts(Vector<String> fileList, ReceiptSettings settings) throws Exception {
		System.out.println( String.format( "Uploading %d receipts..", fileList.size() ));

		Map<String,String> taskIds = new HashMap<String, String>();

		for (int fileIndex = 0; fileIndex < fileList.size(); fileIndex++ ) {
			String filePath = fileList.get(fileIndex);

			File file = new File(filePath);
			String fileBase = file.getName();
			if (fileBase.indexOf(".") > 0) {
				fileBase = fileBase.substring(0, fileBase.lastIndexOf("."));
			}

			System.out.println( filePath );
			Task task = restClient.processReceipt( filePath, settings );
			taskIds.put(task.Id, fileBase +".xml");	
		}
		return taskIds;
	}

	private static Map<String,String> submitRemoteUrls(Vector<String> urlList, ProcessingSettings settings) throws Exception {
		System.out.println( String.format( "Processing %d urls...", urlList.size() ));
		Map<String,String> taskIds = new HashMap<String,String>();

		for (int i = 0; i < urlList.size(); i++ ) {
			String url = urlList.get(i);

			String fileName = url.substring( url.lastIndexOf('/')+1, url.length() );
			String fileBase  = fileName.substring(0, fileName.lastIndexOf('.'));

			System.out.println( url );
			Task task = restClient.processRemoteImage( url, settings );
			taskIds.put(task.Id, fileBase + settings.getOutputFileExt());
		}
		return taskIds;
	}

	/**
	* Wait until tasks are finished and download recognition results
	*/
	private static void waitAndDownloadResults( Map<String,String> taskIds ) throws Exception {
		// Call listFinishedTasks while there are any not completed tasks from taskIds

		// Please note: API call 'listFinishedTasks' returns maximum 100 tasks
		// So, to get all our tasks we need to delete tasks on server. Avoid running
		// parallel programs that are performing recognition with the same Application ID
	
		System.out.println( "Waiting.." );
		
		while ( taskIds.size() > 0 ) {
				Task[] finishedTasks = restClient.listFinishedTasks();

				for ( int i = 0; i < finishedTasks.length; i++ ) {
					Task task = finishedTasks[i];
					if( taskIds.containsKey( task.Id ) ) {
						// Download task
						String fileName = taskIds.remove(task.Id);

						if( task.Status == Task.TaskStatus.Completed ) {
						String outputPath = outputDir + "/" + fileName;
						restClient.downloadResult(task, outputPath);
						System.out.println( String.format( "Ready %s, %d remains", fileName, taskIds.size() ) );
						} else {
							System.out.println( String.format( "Failed %s, %d remains", fileName, taskIds.size() ));
						}

					} else {
						System.out.println( String.format( "Deleting task %s from server", task.Id ) );
					}
					restClient.deleteTask( task.Id );
				}
				Thread.sleep(2000);
		}
	}


	/**
	* Set output directory and create it if necessary
	*/
	private static void setOutputPath(String value) {
		outputDir = value;
		File dir = new File(outputDir);
		if (!dir.exists() ) {
			dir.mkdirs();	
		}
	}


	private static Client restClient;
	private static String outputDir;
}
