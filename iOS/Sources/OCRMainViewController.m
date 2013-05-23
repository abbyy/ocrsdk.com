#import "OCRMainViewController.h"
#import "OCRDemoClient.h"

// Modify this constants to change the recognition languages and export format
static NSString * const kRecognitionLanguages = @"English";
static NSString * const kExportFormat = @"txt";

@implementation OCRMainViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
	self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
	
	if (self != nil) {
		self.title = NSLocalizedString(@"Image", @"Image");
	}
	
	return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
	
	UIBarButtonItem *takePhotoButton = [[UIBarButtonItem alloc] initWithTitle:NSLocalizedString(@"Take Photo", @"Take Photo")
																		style:UIBarButtonItemStyleBordered
																	   target:self
																	   action:@selector(takePhoto:)];
	
    UIBarButtonItem *recognizeButton = [[UIBarButtonItem alloc] initWithTitle:NSLocalizedString(@"Recognize", @"Recognize")
																			style:UIBarButtonItemStyleBordered
																		target:self
																		action:@selector(recognize:)];
	
	self.navigationItem.leftBarButtonItem = takePhotoButton;
	self.navigationItem.rightBarButtonItem = recognizeButton;
	
	self.imageView.image = [UIImage imageNamed:@"sample.jpg"];
}

#pragma mark -

- (void)takePhoto:(id)sender
{
	UIImagePickerController* imagePicker = [[UIImagePickerController alloc] init];
	
	imagePicker.sourceType = [UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera] ? UIImagePickerControllerSourceTypeCamera :  UIImagePickerControllerSourceTypePhotoLibrary;
	imagePicker.mediaTypes = [NSArray arrayWithObject:(NSString *)kUTTypeImage];
	imagePicker.allowsEditing = NO;
	imagePicker.delegate = self;
	
	[self presentModalViewController:imagePicker animated:YES];
}

- (void)recognize:(id)sender
{
	self.alertView = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Authorizing...", @"Authorizing...")
												message:@"\n\n"
											   delegate:self
									  cancelButtonTitle:NSLocalizedString(@"Cancel", @"Cancel")
									  otherButtonTitles:nil];
	
	UIActivityIndicatorView *activityIndicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
	[self.alertView addSubview:activityIndicator];
	[self.alertView show];
	activityIndicator.center = CGPointMake(self.alertView.bounds.size.width / 2, self.alertView.bounds.size.height - 90);
	
	[activityIndicator startAnimating];
	
	[[OCRDemoClient sharedClient] activateInstallationWithDeviceId:[[[UIDevice currentDevice] identifierForVendor] UUIDString] success:^{
		self.alertView.title = NSLocalizedString(@"Uploading image...", @"Uploading image...");
		
		NSData *imageData = UIImageJPEGRepresentation(self.imageView.image, 0.5);
		NSDictionary *processingParams = @{@"language": kRecognitionLanguages, @"exportFormat": kExportFormat};
		
		[[OCRDemoClient sharedClient] startTaskWithImageData:imageData withParams:processingParams success:^(NSDictionary *taskInfo) {
			[self updateTaskStatus:[taskInfo objectForKey:OCRSDKTaskId]];
		} failure:^(NSError *error) {
			[self showError:error];
		}];
	} failure:^(NSError *error) {
		[self showError:error];
	} force:NO];
}

- (void)updateTaskStatus:(NSString *)taskId
{
	self.alertView.title = NSLocalizedString(@"Processing image...", @"Processing image...");
	
	[[OCRDemoClient sharedClient] getTaskInfo:taskId success:^(NSDictionary *taskInfo) {
		NSString *status = [taskInfo objectForKey:OCRSDKTaskStatus];
		
		if ([status isEqualToString:OCRSDKTaskStatusCompleted]) {
			NSString *downloadURLString = [taskInfo objectForKey:OCRSDKTaskResultURL];
			
			[self downloadResult:[NSURL URLWithString:downloadURLString]];
		} else if ([status isEqualToString:OCRSDKTaskStatusProcessingFailed] || [status isEqualToString:OCRSDKTaskStatusNotEnoughCredits]) {
			NSError *error = [NSError errorWithDomain:@"com.abbyy.ocrsdk.demo" code:666 userInfo:@{NSLocalizedDescriptionKey: NSLocalizedString(@"Error processing image", @"Error processing image")}];
			[self showError:error];
		} else {
			[self performSelector:@selector(updateTaskStatus:) withObject:taskId afterDelay:1.0];
		}
	} failure:^(NSError *error) {
		[self showError:error];
	}];
}

- (void)downloadResult:(NSURL *)url
{
	self.alertView.title = NSLocalizedString(@"Downloading result...", @"Downloading result...");
	
	[[OCRDemoClient sharedClient] downloadRecognizedData:url success:^(NSData *downloadedData) {
		[self.alertView dismissWithClickedButtonIndex:-1 animated:YES];
		
		if (self.textViewController == nil) {
	        self.textViewController = [[OCRTextViewController alloc] initWithNibName:@"TextView" bundle:nil];
	    }
		
	    self.textViewController.text = [[NSString alloc] initWithData:downloadedData encoding:NSUTF8StringEncoding];
		
        [self.navigationController pushViewController:self.textViewController animated:YES];
	} failure:^(NSError *error) {
		[self showError:error];
	}];
}

#pragma mark -

- (void)showError:(NSError *)error
{
	if (error.code != NSURLErrorCancelled) {
		[self.alertView dismissWithClickedButtonIndex:-1 animated:YES];
		
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Error", @"Error")
														message:[error localizedDescription]
													   delegate:nil
											  cancelButtonTitle:NSLocalizedString(@"Ok", @"Ok")
											  otherButtonTitles:nil];
		
		[alert show];
	}
}

#pragma mark - UIImagePickerControllerDelegate

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info
{
	self.imageView.image = [info objectForKey:UIImagePickerControllerOriginalImage];
	[picker dismissModalViewControllerAnimated:YES];
}

#pragma mark - UIAlertViewDelegate

- (void)alertView:(UIAlertView *)alertView willDismissWithButtonIndex:(NSInteger)buttonIndex
{
	if(buttonIndex == alertView.cancelButtonIndex) {
		[NSObject cancelPreviousPerformRequestsWithTarget:self];
		[[[OCRDemoClient sharedClient] operationQueue] cancelAllOperations];
	}
}

@end
