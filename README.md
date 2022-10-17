# AspNet_6_GrayLog

To change configuration go to log4net.config file and change the GELFURL to your Gelf URL

 ```
 <appender name="AsyncGelfHttpAppender" type="Gelf4Net.Appender.AsyncGelfHttpAppender, Gelf4Net">
        <url value="GELFURL" />
        <!-- Limit of log lines to buffer for async send. Defaults to 1000-->
        <!-- If we cannot connect to graylog and the queue reaches the buffersize it will dequeue messages from the queue-->
        <bufferSize value="2000" />
        <!-- Number of tasks to use for the async appender. 0 or fewer indicates one task per processor-->
        <threads value="2" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
            <param name="AdditionalFields" value="app:AsyncHttpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="ApplicationName-API" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false" />
            <!--Sets the full_message and short_message to the specified pattern-->
            <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
        </layout>
    </appender>```
