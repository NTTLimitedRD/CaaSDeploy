# CaaSDeploy
Deployment of CaaS infrastructure using JSON templates

## Command Line
Usage: 

*To deploy using a template and parameter file:*

**CaasDeploy.exe** -action Deploy -template *PathToTemplateFile* -parameters *PathToParametersFile* -deploymentLog *PathToDeploymentLogFile* -region *RegionName* -username *CaaSUserName* -password *CaasPassword*

*To delete a deployment using a previous deployment log*:

**CaasDeploy.exe** -action Delete -deploymentLog *PathToDeploymentLogFile* -region *RegionName* -username *CaaSUserName* -password *CaasPassword*

## Template format
The templates are in JSON format and contain two sections: **parameters** and **resources**.

**parameters** contains a single JSON element containing the list of parameter names and their descriptions. The values for the parameters
(which will vary per deployment) go in a separate file.

**resources** contains an array with the list of resources to be deployed. Each element in the array has the following properties.

* **resourceType**: The type of CaaS resource to deploy. Acceptable values (case sensitive) are NetworkDomain, Vlan, Server, FirewallRule, PublicIpBlock, NatRule.
* **resourceId**: A unique identifier for the resource. This isn't used by CaaS, so it's only valid within the template for deploying templates.
* **dependsOn**: An array with the resourceIds for any other resources which must be created before this one.
* **resourceDefinition**: A blob of JSON that is passed to the CloudControl 2.0 API to create the resource (see [documentation](https://community.opsourcecloud.net/Browse.jsp?id=e5b1a66815188ad439f76183b401f026) for syntax).

JSON properties within the **resourceDefinition** may use the following macros to retrieve values from parameters or from the output after creating another resource:

* **$parameters['*paramName*']**: Retrieves the value of the specfied parameter
* **$resources['*resourceId*']._propertyPath_**: Retrieves the value of the requested property for a previously created resource. The properties can be several levels deep, e.g. "$resources['MyVM'].networkInfo.primaryNic.privateIpv4"

## Sample Template
This template deploys a new Network Domain with a VNET, a Public IP Block and a Server. It also creates a NAT rule mapping the 
public IP to the private IP, and opens firewall ports for web and RDP traffic.
```json
{
  "parameters": {
    "myVMName": {
      "description": "The name to use for the Virtual Machine"
    },
    "myNetworkDomainName": {
      "description": "The name to use for the Network Domain"
    },
    "datacenterId": {
      "description": "The region to deploy to"
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
        "imageId": "8bc629a9-8d71-4b1b-8b26-acdc077edab1",
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
