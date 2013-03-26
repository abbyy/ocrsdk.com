#import "ActivationInfo.h"

@implementation ActivationInfo

@synthesize installationID = _installationID;

- (id)initWithData:(NSData*)data
{
	self = [super init];
	
	if (self) {
		NSXMLParser* parser = [[NSXMLParser alloc] initWithData:data];
		[parser setDelegate:self];
		
		if (![parser parse]) {
			self = nil;
		}
	}
	
	return self;
}

- (void)parser:(NSXMLParser*)parser didStartElement:(NSString*)elementName namespaceURI:(NSString*)namespaceURI qualifiedName:(NSString*)qName attributes:(NSDictionary*)attributeDict
{
	isReadingAuthToken = NO;
	
	if ([elementName isEqualToString:@"authToken"]) {
		isReadingAuthToken = YES;
		self.installationID = [NSString string];
	} else if ([elementName isEqualToString:@"error"]) {
		// TODO: handle errors here
	}
}

- (void)parser:(NSXMLParser*)parser foundCharacters:(NSString*)string
{
	if(isReadingAuthToken) {
		self.installationID = [self.installationID stringByAppendingString:string];
	}
}


@end
