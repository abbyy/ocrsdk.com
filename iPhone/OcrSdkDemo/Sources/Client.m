#import "Client.h"
#import "ProcessingOperation.h"
#import "Task.h"

@implementation Client

@synthesize applicationID = _applicationID;
@synthesize password = _password;

@synthesize delegate = _delegate;

- (id)init
{
	return [self initWithApplicationID:nil password:nil];
}

- (id)initWithApplicationID:(NSString *)applicationID password:(NSString *)password
{
	self = [super init];
	
	if (self) {
		_applicationID = applicationID;
		_password = password;
	}
	
	return self;
}

- (void)processImage:(UIImage*)image withParams:(ProcessingParams*)params
{		
	NSParameterAssert(image);
	
	NSURL* processingURL = [NSURL URLWithString:[NSString stringWithFormat:@"http://cloud.ocrsdk.com/processImage?%@", [params urlString]]];
	
	NSMutableURLRequest* processingRequest = [NSMutableURLRequest requestWithURL:processingURL];
	
	[processingRequest setHTTPMethod:@"POST"];
	[processingRequest setValue:@"applicaton/octet-stream" forHTTPHeaderField:@"Content-Type"];
	[processingRequest setHTTPBody:UIImageJPEGRepresentation(image, 0.5)];
	
	HTTPOperation *uploadOperation = [[HTTPOperation alloc] initWithRequest:processingRequest 
																	 target:self 
															 finishedAction:@selector(uploadFinished:)];
	
	[uploadOperation setAuthenticationDelegate:self];
	
	[uploadOperation start];
}

- (void)uploadFinished:(HTTPOperation*)operation
{
	if (operation.error != nil) {
		if ([self.delegate respondsToSelector:@selector(client:didFailedWithError:)]) {
			[self.delegate client:self didFailedWithError:operation.error];
		}
	} else {
		if ([self.delegate respondsToSelector:@selector(clientDidFinishUpload:)]) {
			[self.delegate clientDidFinishUpload:self];
		}
		
		Task* task = [[Task alloc] initWithData:operation.recievedData];
		
		NSParameterAssert(task);
		NSParameterAssert(task.ID);
		
		NSURL* processingURL = [NSURL URLWithString:[NSString stringWithFormat:@"http://cloud.ocrsdk.com/getTaskStatus?taskId=%@", task.ID]];
		
		NSURLRequest *request = [NSURLRequest requestWithURL:processingURL];
		
		ProcessingOperation *processingOperation = [[ProcessingOperation alloc] initWithRequest:request 
																						 target:self 
																				 finishedAction:@selector(processingFinished:)];
		[processingOperation setAuthenticationDelegate:self];
		
		[processingOperation start];
		
	}
}

- (void)processingFinished:(HTTPOperation*)operation
{
	if (operation.error != nil) {
		if ([self.delegate respondsToSelector:@selector(client:didFailedWithError:)]) {
			[self.delegate client:self didFailedWithError:operation.error];
		}
	} else {
		Task* task = [[Task alloc] initWithData:operation.recievedData];
		
		if ([self.delegate respondsToSelector:@selector(clientDidFinishProcessing:)]) {
			[self.delegate clientDidFinishProcessing:self];
		}
		
		NSParameterAssert(task);
		NSParameterAssert(task.downloadURL);
		
		NSURLRequest *request = [NSURLRequest requestWithURL:task.downloadURL];
		
		HTTPOperation *downloadOperation = [[ProcessingOperation alloc] initWithRequest:request 
																				 target:self 
																		 finishedAction:@selector(downloadFinished:)];
		[downloadOperation setAuthenticationDelegate:self];
		
		[downloadOperation start];
	}
}

- (void)downloadFinished:(HTTPOperation*)operation
{
	if (operation.error != nil) {
		if ([self.delegate respondsToSelector:@selector(client:didFailedWithError:)]) {
			[self.delegate client:self didFailedWithError:operation.error];
		}
	} else if ([self.delegate respondsToSelector:@selector(client:didFinishDownloadData:)]) {
		[self.delegate client:self didFinishDownloadData:operation.recievedData];
	}
}

#pragma mark - HTTPOperationAuthenticationDelegate implementation

- (BOOL)httpOperation:(HTTPOperation *)operation canAuthenticateAgainstProtectionSpace:(NSURLProtectionSpace *)protectionSpace
{
	return YES;
}

- (void)httpOperation:(HTTPOperation *)operation didReceiveAuthenticationChallenge:(NSURLAuthenticationChallenge *)challenge
{
	if ([challenge previousFailureCount] == 0) {
		NSURLCredential* credential = [NSURLCredential credentialWithUser:self.applicationID
																 password:self.password
															  persistence:NSURLCredentialPersistenceForSession];
		
		[[challenge sender] useCredential:credential forAuthenticationChallenge:challenge];
	} else {
		[[challenge sender] cancelAuthenticationChallenge:challenge]; 
	}
}

@end
