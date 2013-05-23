#import "OCRAppDelegate.h"
#import "OCRMainViewController.h"

@implementation OCRAppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
	self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
    
	UIViewController *mainViewController = [[OCRMainViewController alloc] initWithNibName:@"MainView" bundle:nil];
	
	self.navigationController = [[UINavigationController alloc] initWithRootViewController:mainViewController];
	
	self.window.rootViewController = self.navigationController;
    [self.window makeKeyAndVisible];
    
	return YES;
}

@end
