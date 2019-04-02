#import "RecognitionViewController.h"
#import "AppDelegate.h"

#error Provide Application ID and Password
// To create an application and obtain a password,
// register at https://cloud.ocrsdk.com/Account/Register
// More info on getting your application id and password at
// https://ocrsdk.com/documentation/faq/#faq3

// Url of processing service. Change to https://cloud-westus.ocrsdk.com
// if your application was created in US location
static NSString* ProcessingServiceUrl = @"https://cloud-eu.ocrsdk.com";
// Name of application you created
static NSString* MyApplicationID = @"my_app_id";
// Password should be sent to your e-mail after application was created
static NSString* MyPassword = @"my_password";

@implementation RecognitionViewController

@synthesize textView;
@synthesize statusLabel;
@synthesize statusIndicator;

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Release any cached data, images, etc that aren't in use.
}

#pragma mark - View lifecycle

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
}

- (void)viewWillAppear:(BOOL)animated
{
	textView.hidden = YES;
	
	statusLabel.hidden = NO;
	statusIndicator. hidden = NO;
	
    [super viewWillAppear:animated];
}

- (void)viewDidAppear:(BOOL)animated
{
	statusLabel.text = @"Loading image...";
	
	UIImage* image = [(AppDelegate*)[[UIApplication sharedApplication] delegate] imageToProcess];
	
	Client *client = [[Client alloc] initWithApplicationID:MyApplicationID password:MyPassword serviceUrl:ProcessingServiceUrl];
	[client setDelegate:self];
	
	ProcessingParams* params = [[ProcessingParams alloc] init];
	
	[client processImage:image withParams:params];
	
	statusLabel.text = @"Uploading image...";
	
    [super viewDidAppear:animated];
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
	return NO;
}

#pragma mark - ClientDelegate implementation

- (void)clientDidFinishUpload:(Client *)sender
{
	statusLabel.text = @"Processing image...";
}

- (void)clientDidFinishProcessing:(Client *)sender
{
	statusLabel.text = @"Downloading result...";
}

- (void)client:(Client *)sender didFinishDownloadData:(NSData *)downloadedData
{
	statusLabel.hidden = YES;
	statusIndicator.hidden = YES;
	
	textView.hidden = NO;
	
	NSString* result = [[NSString alloc] initWithData:downloadedData encoding:NSUTF8StringEncoding];
	
	textView.text = result; 
}

- (void)client:(Client *)sender didFailedWithError:(NSError *)error
{
	UIAlertView* alert = [[UIAlertView alloc] initWithTitle:@"Error"
													message:[error localizedDescription]
												   delegate:nil 
										  cancelButtonTitle:@"Cancel" 
										  otherButtonTitles:nil, nil];
	
	[alert show];
	
	statusLabel.text = [error localizedDescription];
	statusIndicator.hidden = YES;
}

@end
