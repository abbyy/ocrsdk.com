#import "OCRDemoClient.h"

#error Provide Application ID and Password
// To create an application and obtain a password,
// register at http://cloud.ocrsdk.com/Account/Register
// More info on getting your application id and password at
// http://ocrsdk.com/documentation/faq/#faq3
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
