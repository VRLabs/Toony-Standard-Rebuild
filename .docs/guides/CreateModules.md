---
uid: CreateModules
title: Create Modules
---

# Create Modules

Toony Standard RE:Build is a modular shader that uses modules to define features. This means that you can create modules to add features yourself.

First of all, you should check out the [Modular Shader System documentation](https://mss.vrlabs.dev) to get an hold on the basis of creating modules.

## Structure and hooks

Now that you have a basic idea on how the modular shader system works in general, it's time to talk about how it's used in Toony Standard RE:Build and what are the main hooks available.

By default the modular shader asset does not have the `base modules` part editable, because the management of the modules is done via shader inspector to begin with.
Also the modules shown in there are considered `core modules` that make up the base structure of the shader, as well as the base hookups, and that should not be edited.

> [!NOTE]
> If the shader would be left with only core modules, it would still compile fine, but would just have a black output.

> [!WARNING]
> When creating a new variation from the inspector, you have the possibility to change out core modules for the new modular shader assets, but in general unless you know what you're doing (or if specified by some third party as the way to get their modules to work) you should not be doing that, 
> as it may make the shader unusable or break the compatibility to other modules in case some hookups are not available anymore

These core modules exposes quite some hooks, with some of them designed to be targeted by templates, some to be targeted by functions, and some others are code or variables destinations.

These ones are the keywords designed to be used by templated and that need to generate a new variant if they can be toggled on or off:
- `MAIN_PASS`: Hook for the base pass CG code, defined right after some common unity includes. In general this should not be used
- `MAIN_PASS_PRAGMAS`: Hook used to add #pragma features to the base pass
- `ADD_PASS`: Hook for the add pass CG code, defined right after some common unity includes. In general this should not be used
- `ADD_PASS_PRAGMAS`: Hook used to add #pragma features to the add pass
- `SHADOW_PASS`: Hook for the shadow pass CG code, defined right after some common unity includes. In general this should not be used
- `SHADOW_PASS_PRAGMAS`: Hook used to add #pragma features to the shadow pass
- `META_PASS`: Hook for the meta pass CG code, defined right after some common unity includes. In general this should not be used
- `META_PASS_PRAGMAS`: Hook used to add #pragma features to the meta pass
- `ADDITIONAL_PASSES`: Hook used to add more passes after the meta pass, can be used to add whatever type of pass you want

- `VERTEX_DATA`: Hook that is inside the base pass vertex data structure, used to define additional component that need to be inside the vertex structure. It's going to be used for both the base and add pass.
- `FRAGMENT_DATA`: Hook that is inside the base pass fragment data structure, used to define additional component that need to be inside the fragment structure. It's going to be used for both the base and add pass.
- `META_FRAGMENT_DATA`: Same as `FRAGMENT_DATA`, but for the meta pass.
- `FUNCTION_STAGES`: Hook that can be used to define some other pass stages (like a geometry stage for example) in the main and add passes. To get the stage to work you also need to give the proper #pragma in `MAIN_PASS_PRAGMAS` and `ADD_PASS_PRAGMAS`.

These keywords instead are designed to be used as functions target, but they can also be used for some templates if they need to access some local only data, and they don't need to generate a variant if they can be toggled on or off:
- `VERTEX_FUNCTION`: Hook for adding code to the vertex function, if you need to access or set some data for the output you need to use templates. For main and add passes.
- `FRAGMENT_FUNCTION`: Hook for adding code to the fragment function, should be used mainly for functions, as both the input data and output data can be accessed by global variables. For main and add passes.
- `SHADOW_FRAGMENT_FUNCTION`: Same of `FRAGMENT_FUNCTION`, but for the shadow pass.
- `META_VERTEX_FUNCTION`: Same as `VERTEX_FUNCTION`, but for the meta pass.
- `META_FRAGMENT_FUNCTION`: Same as `FRAGMENT_FUNCTION`, but for the meta pass.

These keywords are designed to be targets for function implementations or variables:
- `DEFAULT_VARIABLES`: Hook for the variables for the main and add passes, it's used by default if a keywords for variables of a function is not given.
- `DEFAULT_CODE`: Hook for the function implementation code for the main and add passes, it's used by default if a keyword for function implementation code of a function is not given.
- `SHADOW_VARIABLES`: Hook for the variables for the shadow pass.
- `SHADOW_CODE`: Hook for the function implementation code for the shadow pass.
- `META_VARIABLES`: Hook for the variables for the meta pass.
- `META_CODE`: Hook for the function implementation code for the meta pass.

> [!NOTE]
> Some modules may add other hooks in new places.
> 
> For example an hypothetical outline module may expose hooks for vertex and fragment functions to be placed inside of it.
> 
> Whenever you use hooks that are not part of core modules be sure to define the module that declares it as a required module for your module.

## Standards on modules

While you can pretty much swap and replace all modules that provide the lighting calculations on the shader with custom ones with your own naming conventions, 
it is a better idea to keep a standard so that it's easier to have modules from multiple creators being able to integrate with your own.

The first big standard is: if you don't need to change some structural part of the shader (like adding some data to the vertex or fragment structures because you need them for your module),
you should lean towards using functions, relegating templates to just some bigger stuff, like extra passes definitions, or stuff that is being preferred by a specific hookup.

Another convention is to keep the same conventions for some common variable names that will most likely be used by the shader even when the default modules are not used:
- Albedo (float4): object main color. Alpha is transparency.
- Diffuse (float3): calculated color of the direct diffuse.
- DirectSpecular (float3): calculated color for the direct specular reflection.
- Emission (float3): color of the emission.
- Glossiness (float): how glossy the object should be.
- IndirectDiffuse (float3): calculated color of the indirect diffuse.
- IndirectSpecular (float3): calculated color of the indirect specular.
- Occlusion (float): occlusion value on the surface, 0 = fully occluded, 1 = no occlusion.
- Roughness (float): how rough the object should be, opposite of Glossiness.
- Specular (float): how specular the object is, default is 0.5 and the value should only affect non metallic specular.
- FinalColor (float4): Final result, in the main passes it should always be available without defining a variable for the function.
- FragData (FragmentData): data passed to the fragment function, in the main passes it should always be available without defining a variable for the function.

> [!NOTE]
> These variables are only a part of the ones available normally, and it should be considered more of a guideline in case you want to make modules that work in multiple non standard module combinations.

Another good standard that can be applied to your module is to split the calculation of whatever you're doing to the final assignment/add operation to the final color or the variables you need to add your result.
By doing this you allow other modules to be able to intercept that value and add their own stuff into it if they need to for their own purposes, therefore allowing creators to make other modules that build upon your module.

## The UVSet system

Toony Standard RE:Build comes with a system to allow you to define and use different uv sets where for each uv set you can add uv spaces, either sampled or calculated, that every texture using that uv set can be set to use via inspector, allowing you to add effects to already defined and used textures.

But to be able to use the system the textures you defined need to use a specific macro to retrieve the uv position to sample, and you also need to define a float variable called `*_YourTextureName*_UV` (this variable is going to pass the index of the uv used from the set):

```glsl
// Macro usage
TSR_TRANSFORM_TEX(UvSetToUse, _YourTexture)

// Example sampling
float4 sampledTexture = UNITY_SAMPLE_TEX2D(_YourTexture, TSR_TRANSFORM_TEX(UvSetToUse, _YourTexture));
```

The `UvSetToUse` should be the array of the set you want to use, usually you want to use the `Uvs` one since that's the main uv set available by default from the shader.

 You will also need to define the used uv set in the ui.
 
## Module's UI

Now that you have the module, you need to tell the inspector what options to show to the user. You can modify what is shown by going to `VRLabs > Toony Standard RE:Build > Edit UI for module`.
This window let you select which ui controls to show, and where to show them, as well as defining eventual UV sets.

First add a section, it can be an already existing section or a new one. In case of an already existing one you just need to put in the same name, and set is as permanent if the original one is also permanent.
In case it's a new one, you also have to define which property is used to check if it's enabled or not (in case you want it to always be enabled, you have to set it as a permanent section).

Now you can add the controls you want to add. A control corresponds to an input. There are different type of controls for different type of data, and some of them have also utilities integrated. In [this page](not yet) you have an overview of the controls available by default.

> [!NOTE]
> The ui system is based on the [Simple Shader Inspectors](https:\\ssi.vrlabs.dev) library, and you have the option to create your own controls and have them available to be used here.

For each control you have to give a couple of static informations, and some extra depending on the type of control:
- Name: name of the control, is similar to an id, and is used as reference for localization.
- Append after: name of the control you want this control to be added after. Can be a control in your list, or a control of another module (you can use the left tab to see the control names of a modular shader's ui).
- Type: type of control to use, all the next parameters will be based on the type of control you select.

In general, each control will also ask for the name of the property you want to edit, unless it's a control that only shows text like a label.

> [!NOTE]
> Texture related controls will also have an input for the uv set to use. For this input you should be set it to the id corresponding to the uv set you're using in the code. For the main set this id is `BASESET`.

Aside of controls, you also have the possibility to set values for properties, keywords, override tags, and render queue when the section is disabled.

## Localization

After loading your new module for the first time into the shader, you will notice that the controls of your section of the ui may have weird names as labels.

This is due to the default localization being generated. the shader has generated a localization folder with an english localization file in the same folder your module is located.

You can use this localization file to edit the text of the controls.

You can also duplicate this file, rename it to another language, and edit the display text and tooltip for that language

> [!NOTE]
> The shader needs to have enabled that language in its settings to be a able to display that localization.
