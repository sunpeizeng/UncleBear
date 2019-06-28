//
//  Bridge.m
//  Unity-iPhone
//
//  Created by KevinZhang on 09/03/2017.
//
//

#include "../../lib_sg_projects/lib_common_ios/SG_project_ios/include.h"
//#include "include.h"
#include "SDKBridge.h"

#ifdef __cplusplus
extern "C" {
#endif
    const char* getPTParam(const char* key, const char* defaultVal)
    {
        NSString* param = [PTConfig getConfParameter:[NSString stringWithCString:key encoding: NSUTF8StringEncoding]defValue:[NSString stringWithCString:defaultVal encoding: NSUTF8StringEncoding]];
        
        char* ret = nil;
        if (param)
        {
            ret = (char*)malloc([param length] + 1);
            memcpy(ret, [param UTF8String], ([param length] + 1));
        }
        return ret;
    }
    
    void setEvent(const char* id, const char* json)
    {
        NSData* jsonData = [[NSString stringWithCString:json encoding:NSUTF8StringEncoding] dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableLeaves error:nil];
        
        [IXClick event: [NSString stringWithCString:id encoding:NSUTF8StringEncoding] attributes:dic];
    }
    
    void setEventBegin(const char* id, const char* json)
    {
        NSData* jsonData = [[NSString stringWithCString:json encoding:NSUTF8StringEncoding] dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableLeaves error:nil];
        
        [IXClick eventBegin:[NSString stringWithCString:id encoding:NSUTF8StringEncoding] attributes:dic];
    }
    
    void setEventEnd(const char* id, const char* json)
    {
        NSData* jsonData = [[NSString stringWithCString:json encoding:NSUTF8StringEncoding] dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableLeaves error:nil];
        
        [IXClick eventEnd:[NSString stringWithCString:id encoding:NSUTF8StringEncoding] attributes:dic];
    }
    
    void showWebView(const char* url, float leftMargin, float topMargin, float width, float height)
    {
        [[WebViewHelper sharedHelper] showWebView_url:url x:leftMargin y:topMargin width:width height:height];
    }
    
    void removeWebView()
    {
        [[WebViewHelper sharedHelper] removeWebView];
    }

#ifdef __cplusplus
}
#endif

@implementation SDKBridge

+ (void) initSDK
{
    NSString* appId = @"104";
    NSString* IXAppId = @"530131";
    NSString* UMengKey = @"587450d245297d4853001066";
    
    [PTConfig initPid:appId];
    
    [IXClick clickWithAppId:IXAppId channelId:nil];
    
    [[UMHelper sharedHelper] initUMKey:UMengKey viewControl: [UtilTool getCurrentUIViewController]];
    [[UMHelper sharedHelper] umengMobClick];
}

@end
