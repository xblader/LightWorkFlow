LightWorkFlow
=============

The first question is: why a new workflow engine instead using WF, that´s supported by Microsoft?

1) You are working with legacy applications, whose code was not built inside a good architecture approach. So, adapting the code and architecture to use WF is not an option.

  1.1) By refactoring legacy applications, in order to avoid "ifs" statements.
  
2) You need to test your workflow using unit tests. WF is so dependent on infrastructure, that does this task really painful.

3) There's no infrastructure to use WF in your company. It happens, you can believe.

4) You don't want to see that ugly XML built by WF design tool. Instead, you'd rather a json file. (That's a good reason)

What's LightWorkFlow?

A lightweight workflow to handle activities and status.

A easy way to handle with workflow. There's no need of a big infrastructure as WF. 
A good way to avoid big and heavy design tools, it fits perfectly for simple cases.

The doc is on the way. Until there, become familiar with lightworkflow seeing WorkflowClient project examples.

There are unit tests to help you as well.

http://lordinateurandme.wordpress.com/2014/02/13/tutorial-lightworkflow-part-1-motivations/


For good samples, see links below:

https://github.com/xblader/LightWorkFlow/blob/master/WorkFlowTestException/ExceptionsWorkFlowTest.cs

https://github.com/xblader/LightWorkFlow/blob/master/WorkFlowTest/WorkFlowTest.cs

https://github.com/xblader/LightWorkFlow/blob/master/WorkFlowClient/Program.cs
