[![Build status](https://ci.appveyor.com/api/projects/status/ktrs07n3r9s4m6nm?svg=true)](https://ci.appveyor.com/project/tonybaloney/caasdeploy)

# CaaSDeploy
Deployment of CaaS infrastructure using JSON templates

## Command Line
Usage: 

*To deploy using a template and parameter file:*

**CaasDeploy.exe** -action Deploy -template *PathToTemplateFile* [-parameters *PathToParametersFile*] -deploymentLog *PathToDeploymentLogFile* -region *RegionName* -username *CaaSUserName* -password *CaasPassword*

The -parameters argument can be omited if you don't have any parameters in your template.

The -region argument should use the CaaS region code, e.g. AU or NA. (The regions can be configured in the _app.config_ file.)

*To delete a deployment using a previous deployment log*:

**CaasDeploy.exe** -action Delete -deploymentLog *PathToDeploymentLogFile* -region *RegionName* -username *CaaSUserName* -password *CaasPassword*

## Template format
The templates are in JSON format and contain up to five sections: **metadata** (required), **parameters** (optional), **existingResources** (optional), **resources** (required) and **orchestration** (optional).

**metadata** defines the template scehma version, and user-defined name and description for this template.

**parameters** contains a single JSON element containing the list of parameter names and their descriptions. The values for the parameters
(which will vary per deployment) go in a separate file.

* **type**: The parameter type. Supported values are: _string_ (default)_, integer, boolean, password, caas.dataCenterId, caas.networkDomainId, caas.vlanId_.
* **title**: A user friendly display title.
* **description**: An optional description of the purpose and usage of the parameter.
* **group**: An optional group identifier for the parameter. Supported values are _Environment_ (default) and _Application_.
* **required**: An optional value indicating whether the parameter is required. The default is _true_.
* **minLength**: An optional value specifiyng the minimum string length.
* **maxLength**: An optional value specifiyng the maximum string length.
* **enum**: An optional array of allowed values.
* **value**: The default value (optional).

**outputParameters** defines an optional list of parameters which will be returned to the hosting application. The value property usually uses $resources macros to extract and return details of deployed resources.


**resources** contains an array with the list of resources to be deployed. Each element in the array has the following properties.

* **resourceType**: The type of CaaS resource to deploy. Acceptable values (case sensitive) are _NetworkDomain, Vlan, Server, FirewallRule, PublicIpBlock, NatRule, Node, Pool, PoolMember, VirtualListener_.
* **resourceId**: A unique identifier for the resource. This isn't used by CaaS, so it's only valid within the template for deploying templates.
* **existingCaasId**: A unique identifier for the resource in CaaS if it already exists. This property is optional. If provided, the deployment tool will reuse the existing resource instead of creating a new one and the resourceDefinition property can be omitted.
* **dependsOn**: An array with the resourceIds for any other resources which must be created before this one.
* **resourceDefinition**: A blob of JSON that is passed to the CloudControl 2.0 API to create the resource (see [documentation](https://community.opsourcecloud.net/Browse.jsp?id=e5b1a66815188ad439f76183b401f026) for syntax).
* **scripts**: For Server resources only, used to specify that you want to run scripts on the VM after deployment. Contains two child properties:
  * **bundleFile**: An array of either  script files or a .zip files containing scripts and related files that should be uploaded to the server post-deployment. Zip files will be extracted automatically.
  * **onDeploy**: An array of commands to execute on the VM post deployment.

**orchestration** defines an orchestration process that will be executed after the infrastructure is deployed. The JSON
elmement must contain a **provider** attribute that specifies the .NET type name of the orchestration provider. All other
properties and nested objects will vary depending on the provider implementation.

JSON properties within the template may use the following macros to retrieve values from parameters or from the output after creating another resource:

* **$parameters['*paramName*']**: Retrieves the value of the specfied parameter
* **$resources['*resourceId*']._propertyPath_**: Retrieves the value of the requested property for a previously created resource (including those defined in **existingResources**). The properties can be several levels deep, e.g. "$resources['MyVM'].networkInfo.primaryNic.privateIpv4"
* **$serverImage['*dataCenterId*', '*ImageName*']**: Retrieves the base server image with the supplied name from the supplied data center.
* **$customerImage['*dataCenterId*', '*ImageName*']**: Retrieves the customer server image with the supplied name from the supplied data center.
* **$nextAvailablePublicIP['*networkDomainId*']**: Retrieves the next available Public IP Address for a network domain. A new Public Ip Block will be added only if necessary.

## Sample Template
This template deploys a new Network Domain with a VNET, a Public IP Block and a Server. It also creates a NAT rule mapping the 
public IP to the private IP, and opens firewall ports for web and RDP traffic.
```json
{
  "metadata": {
    "schemaVersion": "0.1",
    "templateName": "Web Server in new Network Domain",
    "templateDescription":  "Deploys a new VM in a new Newtwork Domain and VNET, with a public IP address and NAT rule, and firewall rules for HTTP and RDP traffic."
  },
  "parameters": {
    "myVMName": {
      "title": "VM Name",
      "description": "The name to use for the new Virtual Machine",
      "type": "string"
    },
    "myNetworkDomainName": {
      "title": "Network Domain Name",
      "description": "The name to use for the new Network Domain",
      "type": "string"
    },
    "datacenterId": {
      "title": "Data Center",
      "description": "The region to deploy to",
      "type": "caas.dataCenterId"
    }
  },
  "outputParameters": {
    "serverUrl": {
      "title": "Server URL",
      "description": "The URL to the deployed server's web application.",
      "type": "string",
	  "value": "http://$resources['PublicIpBlock'].baseIp/index.html"
    }
  },
  "resources": [
    {
      "resourceType": "FirewallRule",
      "resourceId": "AllowHTTPFirewallRule",
      "resourceDefinition": {
        "networkDomainId": "$resources['MyNetworkDomain'].id",
        "name": "AllowHTTPFirewallRule",
        "action": "ACCEPT_DECISIVELY",
        "ipVersion": "IPV4",
        "protocol": "TCP",
        "source": {
          "ip": {
            "address": "ANY"
          }
        },
        "destination": {
          "ip": { "address": "$resources['PublicIpBlock'].baseIp" },
          "port": {
            "begin": "80",
            "end": "80"
          }
        },
        "enabled": "true",
        "placement": {
          "position": "FIRST"
        }
      },
      "dependsOn": [
        "PublicIpBlock"
      ]
    },
        {
      "resourceType": "FirewallRule",
      "resourceId": "AllowRDPFirewallRule",
      "resourceDefinition": {
        "networkDomainId": "$resources['MyNetworkDomain'].id",
        "name": "AllowRDPFirewallRule",
        "action": "ACCEPT_DECISIVELY",
        "ipVersion": "IPV4",
        "protocol": "TCP",
        "source": {
          "ip": {
            "address": "ANY"
          }
        },
        "destination": {
          "ip": { "address": "$resources['PublicIpBlock'].baseIp" },
          "port": { "begin": "3389", "end": "3389" }
        },
        "enabled": "true",
        "placement": {
          "position": "FIRST"
        }
      },
    "dependsOn": [
        "PublicIpBlock"
      ]
    },
    {
      "resourceType": "NetworkDomain",
      "resourceId": "MyNetworkDomain",
      "resourceDefinition": {
        "datacenterId": "$parameters['datacenterId']",
        "name": "$parameters['myNetworkDomainName']",
        "description": "Testing CaaS Deployment Templates",
        "type": "ESSENTIALS"
      },
      "dependsOn": [ ]
    },
    {
      "resourceType": "Vlan",
      "resourceId": "VLAN1",
      "resourceDefinition": {
        "networkDomainId": "$resources['MyNetworkDomain'].id",
        "name": "Toms Test VLAN",
        "description": "Testing CaaS Deployment Templates",
        "privateIpv4BaseAddress": "10.0.3.0"
      },
      "dependsOn": [
        "MyNetworkDomain"
      ]
    },
    {
      "resourceType": "Server",
      "resourceId": "MyVM",
      "resourceDefinition": {
        "name": "$parameters['myVMName']",
        "description": "Testing CaaS Deployment Templates",
        "imageId": "$serverImage[$parameters['datacenterId'], 'Ubuntu 12.04 2 CPU']",
        "start": true,
        "administratorPassword": "Password@1",
        "networkInfo": {
          "networkDomainId": "$resources['MyNetworkDomain'].id",
          "primaryNic": { "vlanId": "$resources['VLAN1'].id" },
          "additionalNic": [ ]
        },
        "disk": [
          {
            "scsiId": "0",
            "speed": "STANDARD"
          }
        ]
      },
      "scripts": {
        "bundleFile": [ "TestScripts.zip" ],
        "onDeploy": [ "powershell.exe test1.ps1 -foo $parameters['message']" ]
      },
      "dependsOn": [
        "VLAN1"
      ]
    },
    {
      "resourceType": "PublicIpBlock",
      "resourceId": "PublicIpBlock",
      "dependsOn": [ "MyNetworkDomain", "MyVM" ],
      "resourceDefinition": {
        "networkDomainId": "$resources['MyNetworkDomain'].id"
      }
    },
    {
      "resourceType": "NatRule",
      "resourceId": "nat",
      "dependsOn":  [ "MyNetworkDomain", "PublicIpBlock", "MyVM", "VLAN1"],
      "resourceDefinition": 
      {
          "networkDomainId": "$resources['MyNetworkDomain'].id",
          "internalIp" : "$resources['MyVM'].networkInfo.primaryNic.privateIpv4",
          "externalIp" :"$resources['PublicIpBlock'].baseIp"
      }
    }
  ]
}
```
Note that the template defines three parameters. You'll need to supply a parameter file in the following format.
```json
{
   "parameters": {
        "myVMName": {
            "value": "Toms Test VM"
        },
        "myNetworkDomainName": {
            "value": "Toms New Test Network Domain"
        },
        "datacenterId": {
            "value": "NA9"
        }
    }
}
```

## Using the .NET Library
You can also use the .NET library in your own projects instead of the console application.

First, you need to initialize a new instance of _RuntimeContext_ with _CaasAccountDetails_. If you don't know the details like URLs and OrgID, you can use the _CaasAuthentication_ helper class.
```c#
RuntimeContext runtimeContext = new RuntimeContext
{
    AccountDetails = await CaasAuthentication.Authenticate(userName, password, region),
    LogProvider = new ConsoleLogProvider()
};
```

Next you create an instance of the _TaskBuilder_ class and use it to parse either a deployment template or deployment log file.
```c#
TaskBuilder taskBuilder = new TaskBuilder();
TaskExecutor taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(templateFile, parametersFile);
```

Finally, execute the parsed tasks.
```c#
DeploymentLog log = await taskExecutor.Execute(runtimeContext);
```

Note, the _Execute_ method of the task executor is just a helper method for convenience. If you need more control over the task execution, you can simply enumerate the tasks and execute them individually.
```c#
foreach (var task in taskExecutor.Tasks)
{
    await task.Execute(runtimeContext, taskExecutor.Context);
    if (taskExecutor.Context.Log.Status == DeploymentLogStatus.Failed)
    {
        throw new Exception(taskExecutor.Context.Log.Resources.Last().Error.Message);
    }
}
```

## Terms
This library has been provided as a utility for clients, it does not form part of a service provided by Dimension Data.
This utility is provided 'as-is' and Dimension Data cannot provide support for its usage.
