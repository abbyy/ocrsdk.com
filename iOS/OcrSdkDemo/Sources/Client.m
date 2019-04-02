#import "Client.h"
#import "ProcessingOperation.h"
#import "Task.h"
#import "NSString+Base64.h"

@implementation Client

@synthesize applicationID = _applicationID;
@synthesize password = _password;
@synthesize serviceUrl = _serviceUrl;

@synthesize delegate = _delegate;

- (id)init
{
	return [self initWithApplicationID:nil password:nil serviceUrl:nil];
}

- (id)initWithApplicationID:(NSString *)applicationID password:(NSString *)password serviceUrl:(NSString*)serviceUrl
{
	self = [super init];
	
	if (self) {
		_applicationID = applicationID;
		_password = password;
		_serviceUrl = serviceUrl;
	}
	
	return self;
}

- (NSString*)authString
{
	NSString *encodedCredentials = [[NSString stringWithFormat:@"%@:%@", self.applicationID, self.password] base64EncodedString];
	
	return [NSString stringWithFormat:@"Basic %@", encodedCredentials];
}

- (void)processImage:(UIImage*)image withParams:(ProcessingParams*)params
{		
	NSParameterAssert(image);
	
	NSURL* processImageURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/processImage?%@", self.serviceUrl, [params urlString]]];
	
	NSMutableURLRequest* processingRequest = [NSMutableURLRequest requestWithURL:processImageURL];
	
	[processingRequest setHTTPMethod:@"POST"];
	[processingRequest setValue:@"application/octet-stream" forHTTPHeaderField:@"Content-Type"];
	[processingRequest setHTTPBody:UIImageJPEGRepresentation(image, 0.5)];
	
	[processingRequest setValue:[self authString] forHTTPHeaderField:@"Authorization"];
	
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
		
		Task* task = [[Task alloc] initWithData:operation.receivedData];
		
		NSParameterAssert(task);
		NSParameterAssert(task.ID);
		
		NSURL* getTaskStatusURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/getTaskStatus?taskId=%@", self.serviceUrl, task.ID]];
		
		NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:getTaskStatusURL];
		
		[request setValue:[self authString] forHTTPHeaderField:@"Authorization"];
		
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
		Task* task = [[Task alloc] initWithData:operation.receivedData];
		
		if (!task) {
			NSError* error = [NSError errorWithDomain:@"ABBYYOcr" code:0 userInfo:@{NSLocalizedDescriptionKey:@"Response parse failed"}];
			
			if ([self.delegate respondsToSelector:@selector(client:didFailedWithError:)]) {
				[self.delegate client:self didFailedWithError:error];
			}
			return;
		}
		
		if (task.status != Completed) {
			NSError* error = [NSError errorWithDomain:@"ABBYYOcr" code:0 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"Unexpected task state %d", task.status]}];
			
			if ([self.delegate respondsToSelector:@selector(client:didFailedWithError:)]) {
				[self.delegate client:self didFailedWithError:error];
			}
			return;
		}
		
		if ([self.delegate respondsToSelector:@selector(clientDidFinishProcessing:)]) {
			[self.delegate clientDidFinishProcessing:self];
		}
		
		NSParameterAssert(task);
		NSParameterAssert(task.downloadURL);
		
		NSURLRequest *request = [NSURLRequest requestWithURL:task.downloadURL];
		
		HTTPOperation *downloadOperation = [[ProcessingOperation alloc] initWithRequest:request 
																				 target:self 
																		 finishedAction:@selector(downloadFinished:)];

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
		[self.delegate client:self didFinishDownloadData:operation.receivedData];
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
