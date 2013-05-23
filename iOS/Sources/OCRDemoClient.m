#import "OCRDemoClient.h"

// !!! Provide Application ID and Passworh here
static NSString * const kApplicationId = @"<# your application id #>";
static NSString * const kPassowrd = @"<# your password #>";

@implementation OCRDemoClient

+ (instancetype)sharedClient
{
	static OCRDemoClient* sharedClient;
	static dispatch_once_t onceToken;
	dispatch_once(&onceToken, ^{
		sharedClient = [[OCRDemoClient alloc] initWithApplicationId:kApplicationId password:kPassowrd];
	});
	
	return sharedClient;
}

@end
