<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CEPSite.Default" %>

<!DOCTYPE html>


<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  
    <style>

        .expander.expanded {
            padding-left: 13px;
            background-position: left center;
            background-repeat: no-repeat;
            background-image: url(images/expanded.gif);
        }

        .expander.collapsed {
            padding-left: 13px;
            background-position: left center;
            background-repeat: no-repeat;
            background-image: url(images/collapsed.gif);
        }

    </style>

    <link rel="stylesheet" href="//code.jquery.com/ui/1.11.2/themes/smoothness/jquery-ui.css" />
    <link type="text/css" rel="stylesheet" href="/css/layout-default-latest.css" />
    <link rel="stylesheet" href="https://yui-s.yahooapis.com/pure/0.6.0/pure-min.css" />

    <script src="//code.jquery.com/jquery-1.10.2.js"></script>
    <script src="//code.jquery.com/ui/1.11.2/jquery-ui.js"></script>
	<script type="text/javascript" src="/libraries/jquery.layout-latest.js"></script>
    <script type="text/javascript" src="/libraries/jquery-ui-timepicker-addon.js"></script>
    <script type="text/javascript" src="/libraries/simple-expand.min.js"></script>
    <script src="libraries/RGraph.common.core.js"></script>
    <script src="libraries/RGraph.common.context.js"></script>
    <script src="libraries/RGraph.common.annotate.js"></script>
    <script src="libraries/RGraph.common.tooltips.js"></script>
    <script src="libraries/RGraph.common.zoom.js"></script>
    <script src="libraries/RGraph.common.resizing.js"></script>
    <script src="libraries/RGraph.line.js"></script>

    <script>

        var eventLocations = []; // Array of event guids with x,y coords

        var JSON_EventDataNewsUpdates = <%= this.JSON_NumberOfNewsUpdatesPerPriceUpdate %>;
        var JSON_EventDataNegativeSentiment = <%= this.JSON_NumberOfNegativeSentimentPerPriceUpdate %>;    
        var JSON_EventDataPositiveSentiment = <%= this.JSON_NumberOfPositiveSentimentPerPriceUpdate %>; 
        var JSON_EventDataRestrictedPriceUpdates = <%= this.JSON_NumberOfRestrictedPriceUpdates %>;
        var JSON_EventDataNewsStories = <%= this.JSON_NumberOfNewsStoriesPerPriceUpdate %>;
        var JSON_EventDataPriceJumps = <%= this.JSON_NumberOfPriceJumpsPerPriceUpdate %>;
        var JSON_EventDataPriceFalls = <%= this.JSON_NumberOfPriceFallsPerPriceUpdate %>;
        var JSON_EventDataNewsStoryMinus = <%= this.JSON_NumberOfNewsStoryMinusPerPriceUpdate %>;
        var JSON_EventDataNewsStoryPlus = <%= this.JSON_NumberOfNewsStoryPlusPerPriceUpdate %>;
        var JSON_EventDataNewsUpdateMinus = <%= this.JSON_NumberOfNewsUpdateMinusPerPriceUpdate %>;
        var JSON_EventDataNewsUpdatePlus = <%= this.JSON_NumberOfNewsUpdatePlusPerPriceUpdate %>;

        var JSON_EventDataNewsUpdates_Overlay = <%= this.JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay %>;
        var JSON_EventDataNegativeSentiment_Overlay = <%= this.JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay %>;    
        var JSON_EventDataPositiveSentiment_Overlay = <%= this.JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay %>; 
        var JSON_EventDataRestrictedPriceUpdates_Overlay = <%= this.JSON_NumberOfRestrictedPriceUpdates_Overlay %>;
        var JSON_EventDataNewsStories_Overlay = <%= this.JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay %>;
        var JSON_EventDataPriceJumps_Overlay = <%= this.JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay %>;
        var JSON_EventDataPriceFalls_Overlay = <%= this.JSON_NumberOfPriceFallsPerPriceUpdate_Overlay %>;
        var JSON_EventDataNewsStoryMinus_Overlay = <%= this.JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay %>;
        var JSON_EventDataNewsStoryPlus_Overlay = <%= this.JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay %>;
        var JSON_EventDataNewsUpdateMinus_Overlay = <%= this.JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay %>;
        var JSON_EventDataNewsUpdatePlus_Overlay = <%= this.JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay %>;

        var graph;
        var timeTickIndex = 0;
        var timeTickIndex_Overlay = 0;
            
        function findPos(obj) 
        {
            var curleft = 0, curtop = 0;
            if (obj.offsetParent) {
                do {
                    curleft += obj.offsetLeft;
                    curtop += obj.offsetTop;
                } while (obj = obj.offsetParent);
                return { x: curleft, y: curtop };
            }
            return undefined;
        }

        function Togglegraph()
        {
            var canvasSize = document.getElementById('sliderCanvasSize');
            var canvas = document.getElementsByTagName('canvas')[0];

            canvas.width  = canvasSize.value * 1;
            canvas.height = canvasSize.value * 0.8;

            eventLocations = [];

            timeTickIndex = 0;
            timeTickIndex_Overlay = 0;

            $('#mainCanvas').mousedown(function(e) {
                var pos = findPos(this);
                var x = e.pageX - pos.x;
                var y = e.pageY - pos.y;

                //ajdust for error
                x = x - 2;

                var display = "x=" + x + ", y=" + y + " : ";
                var events = "";
                var lastEvent = "";

                for(var i = 0; i < eventLocations.length; i++)
                {
                    var halfBoxSize = eventLocations[i].boxSize/2;

                    var topLeftX = x - halfBoxSize;
                    var topLeftY = y - halfBoxSize;
                    var bottomRightX =  x + halfBoxSize;
                    var bottomRightY =  y + halfBoxSize;

                    if (eventLocations[i].x >= topLeftX && eventLocations[i].x <= bottomRightX && eventLocations[i].y >= topLeftY && eventLocations[i].y <= bottomRightY)
                    {
                        events += eventLocations[i].JSON_EventArray + ",";
                        lastEvent = eventLocations[i].JSON_EventArray;
                        lastEvent = String(lastEvent).split(",")[0];
                    }
                }

                $('#Hidden_EventToDisplay').attr('value', lastEvent);

                $.ajax({
                    type: "POST",
                    url: "EventHandler.ashx",
                    data: { 'Ids': events},
                    success: OnComplete,
                    error: OnFail
                });

            });

            function OnComplete(result) {
                $('#EventDisplay').html(result);

                var eventMarker = result.indexOf("<%= this.SIMPLE_EVENT_MARKER %>");

                if (eventMarker >= 0 && eventMarker < 40) {
                    // The top event is a simple one
                    $('#EventButtons').hide();
                } else {
                    $('#EventButtons').show();
                }

            }
            function OnFail(result) {
                alert('Request Failed');
            }

            var data = <%= this.JSON_PriceUpdates %>;
            var data_Overlay = <%= this.JSON_PriceUpdates_Overlay %>;

            var data_Combined = [
                data,
                data_Overlay
            ];

            new RGraph.Line({
                id: 'mainCanvas',
                data: data_Combined,
                options: {
                    gutterTop: 50,
                    colors: ['black', 'red'],
                    tickmarks: TimeTick,
                    shadow: false,
                    spline: true,
                    hmargin: 15,
                    linewidth: 3,
                    ylabelsCount: 7,
                    axisColor: 'gray',
                    textSize: 10,
                    textAccessible: true,
                    ticksize: 10,
                    numyticks: 7,
                    backgroundGridAutofitNumhlines: 10,
                    backgroundGridAutofitNumvlines: 4,
                    numvlines: <% = this.NumberOfDates %>,
                    key: ['<%= this.ProductName %>'],
                    keyPosition: 'gutter',
                    labels: <%= this.JSON_DateLabels %>,
                    ymin: <%= Engine.Lowest_Y %>,
                    ymax: <%= Engine.Highest_Y %>
                }
            }).draw();
        }

        function ResizeGraph()
        {
            timeTickIndex = 0;

            var canvasSize = document.getElementById('sliderCanvasSize');
            var canvas = document.getElementsByTagName('canvas')[0];

            canvas.width  = canvasSize.value * 1;
            canvas.height = canvasSize.value * 0.8;

            Togglegraph();
        }
            
        function DrawEventBox(obj, numberOfEvents, JSON_EventArray, boxSize, fillStyle, x, y)
        {
            var context = obj.context;

            if (numberOfEvents > 0)
            {
                for (var eventOffset = 0; eventOffset < numberOfEvents; eventOffset++)
                {
                    context.globalAlpha = 0.5;
                    context.fillStyle = fillStyle;

                    var topLeftX = x - (boxSize/2);
                    var topLeftY = (y - boxSize/2) - (eventOffset * boxSize);

                    context.fillRect(topLeftX, topLeftY, boxSize, boxSize);

                    context.globalAlpha = 1;
                    eventLocations.push({"x" : topLeftX, "y" : topLeftY, "boxSize" : boxSize, "JSON_EventArray" : JSON_EventArray});
                }
            }
        }

        function DrawEventBoxGradient(obj, numberOfEvents, JSON_EventArray, boxSize, color1, color2, x, y)
        {
            var c = document.getElementById("mainCanvas");
            var context = c.getContext("2d");

            if (numberOfEvents > 0)
            {
                for (var eventOffset = 0; eventOffset < numberOfEvents; eventOffset++)
                {
                    var topLeftX = x - (boxSize/2);
                    var topLeftY = (y - boxSize/2) - (eventOffset * boxSize);

                    //var gradient=context.createLinearGradient(250,250,300,300);
                    //var gradient = context.createLinearGradient(topLeftX, topLeftY, boxSize, boxSize);

                    var gradient = context.createRadialGradient(topLeftX+(boxSize/2), topLeftY+(boxSize/2), boxSize/4 ,topLeftX+(boxSize/2), topLeftY+(boxSize/2),boxSize/2 );

                    gradient.addColorStop(0,color1);
                    gradient.addColorStop(1,color2);

                    context.globalAlpha = 0.5;

                    context.fillStyle =  gradient;
                    //context.rect(topLeftX, topLeftY, boxSize, boxSize);

                    context.fillRect(topLeftX, topLeftY, boxSize, boxSize);
                    context.fill();

                    context.strokeStyle = color1;
                    context.lineWidth = Math.ceil(boxSize/20);
                    context.strokeRect(topLeftX, topLeftY, boxSize, boxSize);

                    context.globalAlpha = 1;
                    eventLocations.push({"x" : topLeftX, "y" : topLeftY, "boxSize" : boxSize, "JSON_EventArray" : JSON_EventArray});
                }
            }
        }

        function TimeTick_Overlay(obj, data, value, index, x, y, color, prevX, prevY)
        {
            if($('#CheckBox_ShowNewsUpdates_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsUpdates_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsUpdates_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsUpdates_OVERLAY").val(), $("#Color_NewsUpdates_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowPriceUpdates_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataRestrictedPriceUpdates_Overlay[timeTickIndex_Overlay], JSON_EventDataRestrictedPriceUpdates_Overlay[timeTickIndex_Overlay+1], $("#Range_PriceUpdates_OVERLAY").val(), $("#Color_PriceUpdates_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowNewsStories_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStories_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsStories_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsStories_OVERLAY").val(), $("#Color_NewsStories_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowPriceJumps_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPriceJumps_Overlay[timeTickIndex_Overlay], JSON_EventDataPriceJumps_Overlay[timeTickIndex_Overlay+1], $("#Range_PriceJumps_OVERLAY").val(), $("#Color_PriceJumps_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowPriceFalls_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPriceFalls_Overlay[timeTickIndex_Overlay], JSON_EventDataPriceFalls_Overlay[timeTickIndex_Overlay+1], $("#Range_PriceFalls_OVERLAY").val(), $("#Color_PriceFalls_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowNewsStoryPlus_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStoryPlus_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsStoryPlus_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsStoryPlus_OVERLAY").val(), $("#Color_NewsStoryPlus_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowNewsStoryMinus_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStoryMinus_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsStoryMinus_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsStoryMinus_OVERLAY").val(), $("#Color_NewsStoryMinus_OVERLAY").val(), x, y);

            if($('#CheckBox_ShowNewsUpdatePlus_OVERLAY').is(':checked'))
                DrawEventBoxGradient(obj, JSON_EventDataNewsUpdatePlus_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsUpdatePlus_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsUpdatePlus_OVERLAY").val(), $("#Color_PriceJumps_OVERLAY").val(), $("#Color_NewsUpdates_OVERLAY").val(), x, y);
                
            if($('#CheckBox_ShowNewsUpdateMinus_OVERLAY').is(':checked'))
                DrawEventBoxGradient(obj, JSON_EventDataNewsUpdateMinus_Overlay[timeTickIndex_Overlay], JSON_EventDataNewsUpdateMinus_Overlay[timeTickIndex_Overlay+1], $("#Range_NewsUpdateMinus_OVERLAY").val(), $("#Color_PriceFalls_OVERLAY").val(), $("#Color_NewsUpdates_OVERLAY").val(), x, y);

            if($('#CheckBox_ShowNegativeSentiment_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNegativeSentiment_Overlay[timeTickIndex_Overlay], JSON_EventDataNegativeSentiment_Overlay[timeTickIndex_Overlay+1], $("#Range_NegativeSentiment_OVERLAY").val(), $("#Color_NegativeSentiment_OVERLAY").val(), x, y);

            if($('#CheckBox_ShowPositiveSentiment_OVERLAY').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPositiveSentiment_Overlay[timeTickIndex_Overlay], JSON_EventDataPositiveSentiment_Overlay[timeTickIndex_Overlay+1], $("#Range_PositiveSentiment_OVERLAY").val(), $("#Color_PositiveSentiment_OVERLAY").val(), x, y);

            timeTickIndex_Overlay+=2;
        }

        function TimeTick(obj, data, value, index, x, y, color, prevX, prevY) {

            if (color === "red") {

                TimeTick_Overlay(obj, data, value, index, x, y, color, prevX, prevY);

            }

            if($('#CheckBox_ShowNewsUpdates').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsUpdates[timeTickIndex], JSON_EventDataNewsUpdates[timeTickIndex+1], $("#Range_NewsUpdates").val(), $("#Color_NewsUpdates").val(), x, y);
                
            if($('#CheckBox_ShowPriceUpdates').is(':checked'))
                DrawEventBox(obj, JSON_EventDataRestrictedPriceUpdates[timeTickIndex], JSON_EventDataRestrictedPriceUpdates[timeTickIndex+1], $("#Range_PriceUpdates").val(), $("#Color_PriceUpdates").val(), x, y);
                
            if($('#CheckBox_ShowNewsStories').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStories[timeTickIndex], JSON_EventDataNewsStories[timeTickIndex+1], $("#Range_NewsStories").val(), $("#Color_NewsStories").val(), x, y);
                
            if($('#CheckBox_ShowPriceJumps').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPriceJumps[timeTickIndex], JSON_EventDataPriceJumps[timeTickIndex+1], $("#Range_PriceJumps").val(), $("#Color_PriceJumps").val(), x, y);
                
            if($('#CheckBox_ShowPriceFalls').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPriceFalls[timeTickIndex], JSON_EventDataPriceFalls[timeTickIndex+1], $("#Range_PriceFalls").val(), $("#Color_PriceFalls").val(), x, y);
                
            if($('#CheckBox_ShowNewsStoryPlus').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStoryPlus[timeTickIndex], JSON_EventDataNewsStoryPlus[timeTickIndex+1], $("#Range_NewsStoryPlus").val(), $("#Color_PriceJumps").val(), $("#Color_NewsStories").val(), x, y);
                
            if($('#CheckBox_ShowNewsStoryMinus').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNewsStoryMinus[timeTickIndex], JSON_EventDataNewsStoryMinus[timeTickIndex+1], $("#Range_NewsStoryMinus").val(), $("#Color_PriceJumps").val(), $("#Color_NewsStories").val(), x, y);

            if($('#CheckBox_ShowNewsUpdatePlus').is(':checked'))
                DrawEventBoxGradient(obj, JSON_EventDataNewsUpdatePlus[timeTickIndex], JSON_EventDataNewsUpdatePlus[timeTickIndex+1], $("#Range_NewsUpdatePlus").val(), $("#Color_PriceJumps").val(), $("#Color_NewsUpdates").val(), x, y);
                
            if($('#CheckBox_ShowNewsUpdateMinus').is(':checked'))
                DrawEventBoxGradient(obj, JSON_EventDataNewsUpdateMinus[timeTickIndex], JSON_EventDataNewsUpdateMinus[timeTickIndex+1], $("#Range_NewsUpdateMinus").val(), $("#Color_PriceFalls").val(), $("#Color_NewsUpdates").val(), x, y);

            if($('#CheckBox_ShowNegativeSentiment').is(':checked'))
                DrawEventBox(obj, JSON_EventDataNegativeSentiment[timeTickIndex], JSON_EventDataNegativeSentiment[timeTickIndex+1], $("#Range_NegativeSentiment").val(), $("#Color_NegativeSentiment").val(), x, y);

            if($('#CheckBox_ShowPositiveSentiment').is(':checked'))
                DrawEventBox(obj, JSON_EventDataPositiveSentiment[timeTickIndex], JSON_EventDataPositiveSentiment[timeTickIndex+1], $("#Range_PositiveSentiment").val(), $("#Color_PositiveSentiment").val(), x, y);

            timeTickIndex+=2;
        }

        $(function() {

            if('<%= Session["MainPanel"].ToString()%>' != '')
                $("#MainPanel").show();

            if ('<%= Session["OverlayOn"].ToString()%>' != '')
                $("#Overlay").show();

            BindDatePickers();
            BindTimePickers();

            $('body').layout({ applyDemoStyles: true });

            $('.expander').simpleexpand();

            if (!$('#<%= StageOne_CheckBox_ShowPriceUpdates.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowPriceUpdates').hide();
                $('#CheckBox_ShowPriceUpdates').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowPriceUpdates.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowPriceUpdates').hide();
                        $('#CheckBox_ShowPriceUpdates').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowPriceUpdates').attr('checked', true);
                        $('#EVENTS_TR_ShowPriceUpdates').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsUpdates.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsUpdates').hide();
                $('#CheckBox_ShowNewsUpdates').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsUpdates.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsUpdates').hide();
                        $('#CheckBox_ShowNewsUpdates').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsUpdates').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsUpdates').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowPriceJumps.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowPriceJumps').hide();
                $('#CheckBox_ShowPriceJumps').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowPriceJumps.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowPriceJumps').hide();
                        $('#CheckBox_ShowPriceJumps').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowPriceJumps').attr('checked', true);
                        $('#EVENTS_TR_ShowPriceJumps').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowSentimentMinus.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNegativeSentiment').hide();
                $('#CheckBox_ShowNegativeSentiment').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowSentimentMinus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNegativeSentiment').hide();
                        $('#CheckBox_ShowNegativeSentiment').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNegativeSentiment').attr('checked', true);
                        $('#EVENTS_TR_ShowNegativeSentiment').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowSentimentPlus.ClientID %>').is(':checked')) {

                $('#CheckBox_ShowPositiveSentiment').attr('checked', true);
                $('#EVENTS_TR_ShowPositiveSentiment').show();
            }

            $('#<%= StageOne_CheckBox_ShowSentimentPlus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowPositiveSentiment').hide();
                        $('#CheckBox_ShowPositiveSentiment').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowPositiveSentiment').attr('checked', true);
                        $('#EVENTS_TR_ShowPositiveSentiment').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsStories.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsStories').hide();
                $('#CheckBox_ShowNewsStories').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsStories.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsStories').hide();
                        $('#CheckBox_ShowNewsStories').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsStories').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsStories').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowPriceFalls.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowPriceFalls').hide();
                $('#CheckBox_ShowPriceFalls').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowPriceFalls.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowPriceFalls').hide();
                        $('#CheckBox_ShowPriceFalls').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowPriceFalls').attr('checked', true);
                        $('#EVENTS_TR_ShowPriceFalls').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsUpdatePlus.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsUpdatePlus').hide();
                $('#CheckBox_ShowNewsUpdatePlus').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsUpdatePlus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsUpdatePlus').hide();
                        $('#CheckBox_ShowNewsUpdatePlus').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsUpdatePlus').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsUpdatePlus').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsUpdateMinus.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsUpdateMinus').hide();
                $('#CheckBox_ShowNewsUpdateMinus').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsUpdateMinus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsUpdateMinus').hide();
                        $('#CheckBox_ShowNewsUpdateMinus').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsUpdateMinus').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsUpdateMinus').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsStoryPlus.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsStoryPlus').hide();
                $('#CheckBox_ShowNewsStoryPlus').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsStoryPlus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsStoryPlus').hide();
                        $('#CheckBox_ShowNewsStoryPlus').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsStoryPlus').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsStoryPlus').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowNewsStoryMinus.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowNewsStoryMinus').hide();
                $('#CheckBox_ShowNewsStoryMinus').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowNewsStoryMinus.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowNewsStoryMinus').hide();
                        $('#CheckBox_ShowNewsStoryMinus').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowNewsStoryMinus').attr('checked', true);
                        $('#EVENTS_TR_ShowNewsStoryMinus').show();

                    }
                });

            if (!$('#<%= StageOne_CheckBox_ShowPriceUpdates.ClientID %>').is(':checked')) {

                $('#EVENTS_TR_ShowPriceUpdates').hide();
                $('#CheckBox_ShowPriceUpdates').attr('checked', false);
            }

            $('#<%= StageOne_CheckBox_ShowPriceUpdates.ClientID %>')
                .click(function() {

                    if (!$(this).is(':checked')) {

                        $('#EVENTS_TR_ShowPriceUpdates').hide();
                        $('#CheckBox_ShowPriceUpdates').attr('checked', false);

                    } else {

                        $('#CheckBox_ShowPriceUpdates').attr('checked', true);
                        $('#EVENTS_TR_ShowPriceUpdates').show();

                    }
                });
        });

            
        function BindDatePickers() {

            // date pickers for all inputs with .Datepicker css class
            $("#TextBox_StartDate").datepicker({
                numberOfMonths: 1,
                showButtonPanel: true,
                changeMonth: true,
                changeYear: true,
                showOn: "both",
                dateFormat: "dd/mm/yy",
                buttonImage: "/images/Calendar_scheduleHS.png",
                buttonImageOnly: true,
                buttonText: "Pick a date"
            });

            // date pickers for all inputs with .Datepicker css class
            $("#TextBox_EndDate").datepicker({
                numberOfMonths: 1,
                showButtonPanel: true,
                changeMonth: true,
                changeYear: true,
                showOn: "both",
                dateFormat: "dd/mm/yy",
                buttonImage: "/images/Calendar_scheduleHS.png",
                buttonImageOnly: true,
                buttonText: "Pick a date"
            });
        }

        function BindTimePickers() {

            // date pickers for all inputs with .Datepicker css class
            $("#TextBox_StartTime").timepicker({
                controlType: "select",
                showButtonPanel: true,
                showOn: "both",
                timeFormat: "hh:mm tt",
                buttonImage: "/images/clock.png",
                buttonImageOnly: true,
                buttonText: "Pick a time",
                stepMinute: 15
            });

            $("#TextBox_EndTime").timepicker({
                controlType: "select",
                showButtonPanel: true,
                showOn: "both",
                timeFormat: "hh:mm tt",
                buttonImage: "/images/clock.png",
                buttonImageOnly: true,
                buttonText: "Pick a time",
                stepMinute: 15
            });
        }

    </script>

</head>  

<body>
    <form id="form1" runat="server">
    

        <input type="hidden" id="Hidden_EventToDisplay" name="EventToDisplay" />

        <div class="ui-layout-center">
             <canvas id="mainCanvas" width="900" height="800"></canvas>
        </div>
        <div class="ui-layout-east">
            
            <div style="width: 100%; height: 100%;">
                <div id="EventDisplay" style="width: 100%; background-color: #dddddd; padding-left: 5px; padding-top: 5px;">
                </div>
            </div>
        </div>
        <div class="ui-layout-west">

            <div class="expand-container">
                <br/>
                <a class="expander" href="#">Simple Event Feeds</a> 
                    <br/>
                    <br/>
                    <div class="content" style="width: 850px">

                        <table class="pure-table" style="width: 800px;">
                        <thead>
                            <tr>
                                <td style="width: 10px"><input type="checkbox" onclick="$('.FEED').find('*').prop('checked', $('#Checkbox_Stage1Feeds').is(':checked'));" checked="checked" id="Checkbox_Stage1Feeds"/></td>
                                <td>Type</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr style="background-color: #fdf5e6">
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowPriceUpdates" checked="true" Class="FEED"/></td>
                                                                <td style="background-color: #fdf5e6">
                                    <span style="background-color: #fdf5e6">            
                                    <a class="expander" href="#" >Price Updates (ROOT Events)</a> 
                                        <div class="content" style="width: 300px">
                                            <br/>
                                            <table>
                                            <tr >
                                                <td style="text-align: right; padding-right: 10px;"><b>Product</b></td>
                                                <td><asp:TextBox ID="TextBox_ProductName" runat="server" Text="BHP.AX" CssClass="CurvedControl"></asp:TextBox></td>
                                            </tr>

                                            <tr>
                                                <td style="text-align: right; padding-right: 10px;"><br /></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="text-align: right; padding-right: 10px;"><b>From</b></td>
                                                <td><asp:TextBox ID="TextBox_StartDate" runat="server" Text="01/02/2011"  CssClass="DatepickerEndDate CurvedControl50"></asp:TextBox>&nbsp;&nbsp;&nbsp;<asp:TextBox ID="TextBox_StartTime" runat="server" Text="00:00"  CssClass="TimepickerEndDate CurvedControl50"></asp:TextBox></td>
                                            </tr>
                                            <tr>
                                                <td style="text-align: right; padding-right: 10px;"><b>To</b></td>
                                                <td><asp:TextBox ID="TextBox_EndDate" runat="server" Text="01/02/2011" CssClass="DatepickerEndDate CurvedControl50"></asp:TextBox>&nbsp;&nbsp;&nbsp;<asp:TextBox ID="TextBox_EndTime" runat="server" Text="02:45" CssClass="TimepickerEndDate CurvedControl50" ></asp:TextBox></td>
                                            </tr>

                                            <tr>
                                                <td style="text-align: right; padding-right: 10px;"><br /></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="text-align: right; padding-right: 10px;"><b>Inverval</b></td>
                                                <td><asp:TextBox ID="TextBox_Seconds" runat="server" Text="60" CssClass="CurvedControl50"></asp:TextBox> Seconds</td>
                                            </tr>
                                        </table>
                                    </div>
                                    </span>    
                                </td>
                            </tr>
                            <tr class="pure-table-odd">
                                
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsUpdates" checked="true" Class="FEED" /></td>
                                <td>News Updates</td>
                            </tr>
                            <tr>
                                
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowSentimentMinus" checked="true" Class="FEED" /></td>
                                <td>Sentiment [-]</td>
                            </tr>
                            <tr class="pure-table-odd">
                                
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowSentimentPlus" checked="true" Class="FEED" /></td>
                                <td>Sentiment [+]</td>
                            </tr>
                        </tbody>
                    </table>
                        <br/>
                        <br/>
                    </div>
            </div>

            <div class="expand-container">    
                <a class="expander" href="#">Complex Event Types</a> 
                <br/>
                <br/>
                <div class="content" style="width: 100%;">
                    <table class="pure-table">
                        <thead>
                            <tr>
                                <td><input type="checkbox" onclick="$('.PATTERN').find('*').prop('checked', $('#Checkbox_Stage1Patterns').is(':checked'));" checked="checked" id="Checkbox_Stage1Patterns"/></td>
                                <td style="width: 500px;">Type</td>
                                <td style="width: 250px;">Threshold</td>
                                <td style="width: 300px;">
                                    Simple Events
                                </td>
                                <td style="width: 300px;">
                                    Complex Events
                                </td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowPriceJumps" checked="true" Class="PATTERN" /></td>
                                <td>
                                    <a class="expander" href="#">Price Jump</a>
                                    <div class="content">
                                        (p2.LIST_PRICE - p1.LIST_PRICE) > (THRESHHOLD * p1.LIST_PRICE) where p1 and p2 are popped from EVENTSTACK until p2.TimeStamp > p1.TimeStamp
                                    </div>
                                </td>
                                <td><asp:TextBox runat="server" ID="TextBox_PriceChangedPositive_THRESHHOLD" Text="0.0005"></asp:TextBox></td>
                                <td>
                                    Price Update p1<br/>
                                    Price Update p2
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr class="pure-table-odd">
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowPriceFalls" checked="true" Class="PATTERN" /></td>
                                <td>
                                    <a class="expander" href="#">Price Fall</a>
                                    <div class="content">
                                        (p1.LIST_PRICE - p2.LIST_PRICE) > (THRESHHOLD * p1.LIST_PRICE) where p1 and p2 are popped from EVENTSTACK until p2.TimeStamp > p1.TimeStamp
                                    </div>
                                </td>
                                
                                <td><asp:TextBox runat="server" ID="TextBox_PriceChangedNegative_THRESHHOLD" Text="0.0005"></asp:TextBox></td>
                                <td>
                                    Price Update p1<br/>
                                    Price Update p2
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsStories" checked="true" Class="PATTERN" /></td>
                                <td>News Story</td>
                                
                                <td></td>
                                <td>
                                    2 x News Updates
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr class="pure-table-odd">
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsUpdatePlus" checked="true" Class="PATTERN"  /></td>
                                <td>
                                    <a class="expander" href="#">News Update [+]</a>
                                    <div class="content">
                                        A News Update has happened up to THRESHOLD minutes before price jump
                                    </div>
                                </td>
                               
                                <td><asp:TextBox runat="server" ID="TextBox_PriceChangedPositiveWithNewsUpdate_THRESHHOLD" Text="30" /></td>
                                <td>
                                    News Update
                                </td>
                                <td>
                                    Price Jump
                                </td>
                            </tr>
                            <tr>
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsUpdateMinus" checked="true" Class="PATTERN" /></td>
                                <td>
                                    <a class="expander" href="#">News Update [-]</a>
                                    <div class="content">
                                        A News Update has happened up to THRESHOLD minutes before price fall
                                    </div>
                                </td>
                                
                                <td><asp:TextBox runat="server" ID="TextBox_PriceChangedNegativeWithNewsUpdate_THRESHHOLD" Text="30" /></td>
                                <td>
                                    News Update
                                </td>
                                <td>
                                    Price Fall
                                </td>
                            </tr>
                            <tr class="pure-table-odd">
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsStoryPlus" checked="true" Class="PATTERN" /></td>
                                <td>News Story [+]</td>
                                
                                <td></td>
                                <td>
                                </td>
                                <td>
                                    News Story<br/>
                                    Price Jump
                                </td>
                            </tr>
                            <tr>
                                <td><asp:CheckBox runat="server" ID="StageOne_CheckBox_ShowNewsStoryMinus" checked="true" Class="PATTERN" /></td>
                                <td>News Story [-]</td>
                                
                                <td></td>
                                <td>
                                </td>
                                <td>
                                    News Story<br/>
                                    Price Jump
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <br/>
            <asp:Button ID="Button_GetData" runat="server" Text="Get Data" onclick="Button_GetData_Click"  style="color: Green" width="150px" height="25px" /><br />
            
            <div id="MainPanel" style="display: none;">
                <br><hr><br>
            
                <b>Graph Size:</b> <input id="sliderCanvasSize" type="range" min="100" max="2000" step="10" value="1000" /><br /><br />
                <input type="button" value="Generate Graph" onclick="Togglegraph();" id="cb1" style="color: Green; width: 150px; height: 25px"/>

                <div id="EventButtons" style="display: none">
                    <br/>
                    <asp:Button type="button" Text="All Events" id="Button_ShowAllEvents" 
                                runat="server" style="color: Red"  width="150px" height="25px" onclick="Button_ShowAllEvents_Click" /><br />
                    <br />
                    <asp:Button type="button" Text="Show Event" id="Button_ShowIndividualEvent" runat="server" style="color: Red"  width="150px" height="25px" onclick="Button_ShowIndividualEvent_Click" /><br />
                    <br />        
                    <asp:Button type="button" Text="Event and Children" 
                                    id="Button_ShowIndividualEventWithSubEvents" runat="server" style="color: Red" 
                                    onclick="Button_ShowIndividualEventWithSubEvents_Click"  width="150px" height="25px"  />
                </div>
                <br />
                <br/>
                <hr />

                <div class="expand-container" >
                <a class="expander" href="#">Data [<%= AIO_1_StartDateTime %>] - [<%= AIO_1_EndDateTime %>]</a>
                <br/>                
                    <div class="content" style="width: 100%;">
                        <br/>
                        <table class="pure-table">
                        <thead>
                            <tr>
                                <td>Type</td>
                                <td>#</td>
                                <td><input type="checkbox" onclick="$('.EVENTS').prop('checked', $('#Checkbox_Stage2Events').is(':checked'));" checked="checked" id="Checkbox_Stage2Events"/></td>
                                <td>Size</td>
                                <td>Color</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr id="EVENTS_TR_ShowPriceUpdates" class="pure-table-odd">
                                <td>
                                    <a class="expander" href="#">Price Updates</a>
                                    <div class="content" style="width: 200px; overflow: auto">
                                        <%= SearchHTML_PriceUpdates %>
                                    </div>
                                </td>
                                <td><%= this.NumPriceUpdates %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceUpdates" class="EVENTS" checked="checked" /></td>
                                <td><input id="Range_PriceUpdates" type="range" min="1" max="100" step="5" value="1" style="width: 60px"/></td>
                                <td><input id= "Color_PriceUpdates" type="color" value="#00000" class="CurvedControl50" style="width: 30px" /></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdates" >
                                <td>
                                    <a class="expander" href="#">News Updates</a>
                                    <div class="content" style="width: 200px; overflow: auto">
                                        <%= SearchHTML_NewsUpdates%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsUpdates %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdates" class="EVENTS" checked="checked" /></td>
                                <td><input id="Range_NewsUpdates" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NewsUpdates" type="color" value="#ffff00" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNegativeSentiment" class="pure-table-odd">
                                <td>
                                    <a class="expander" href="#">Sentiment [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NegativeSentiment%>
                                    </div>
                                </td>
                                <td><%= this.NumNegativeSentiment %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNegativeSentiment" checked="true" class="EVENTS" /></td>
                                <td><input id="Range_NegativeSentiment" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NegativeSentiment" type="color" value="#444444" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPositiveSentiment">
                                <td>
                                    <a class="expander" href="#">Sentiment [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_PositiveSentiment%>
                                    </div>
                                </td>
                                <td><%= this.NumPositiveSentiment %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPositiveSentiment" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_PositiveSentiment" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PositiveSentiment" type="color" value="#bbbbbb" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPriceFalls">
                                <td>                                            
                                    <a class="expander" href="#">Price Falls</a>
                                    <div class="content">
                                        <%= SearchHTML_PriceFalls%>
                                    </div>
                                </td>
                                <td><%= this.NumPriceFalls %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceFalls" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_PriceFalls" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PriceFalls" type="color" value="#ff0000" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPriceJumps">
                                <td>                                            
                                    <a class="expander" href="#">Price Jumps</a>
                                    <div class="content">
                                        <%= SearchHTML_PriceJumps%>
                                    </div>
                                </td>
                                <td><%= this.NumPriceJumps %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceJumps" class="EVENTS" checked="checked" /></td>
                                <td><input id="Range_PriceJumps" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PriceJumps" type="color" value="#00ff00" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdateMinus">
                                <td>
                                    <a class="expander" href="#">News Update [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsUpdateMinus%>
                                    </div>
                                </td>
                                <td><%=  this.NumNewsUpdateMinus %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdateMinus" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_NewsUpdateMinus" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsUpdateMinus" type="color" value="#FF8000" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdatePlus">
                                <td>
                                    <a class="expander" href="#">News Update [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsUpdatePlus%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsUpdatePlus %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdatePlus" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_NewsUpdatePlus" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsUpdatePlus" type="color" value="#87FF00" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStories">
                                <td>                                            
                                    <a class="expander" href="#">News Stories</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStories%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStories %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStories" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_NewsStories" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NewsStories" type="color" value="#878740" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStoryMinus">
                                <td>
                                    <a class="expander" href="#">News Story [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStoryMinus%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStoryMinus %> </td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStoryMinus" checked="true" Class="EVENTS" /></td>
                                <td><input id="Range_NewsStoryMinus" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsStoryMinus" type="color" value="#DF6020" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStoryPlus">
                                <td>
                                    <a class="expander" href="#">News Story [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStoryPlus%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStoryPlus %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStoryPlus" checked="true" Class="EVENTS"/></td>
                                <td><input id="Range_NewsStoryPlus" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsStoryPlus" type="color" value="#60DF20" class="CurvedControl50">--%></td>
                            </tr>
                        </tbody>
                    </table>
                    </div>
            </div>

                <br>
                <asp:Button type="button" Text="Save Overlay" id="Button_SaveOverlay" runat="server" style="color: Red"  width="150px" height="25px" OnClick="Button_SaveOverlay_Click" />
                <br><br/><hr><br>
            </div>
            <div id="Overlay" style="display: none;">
                <div class="expand-container" >
                    <a class="expander" href="#">
                        AIO [<%= AIO_2_StartDateTime %>] - [<%= AIO_2_EndDateTime %>]
                    </a>
                    <div class="content" style="width: 100%;">
                        <br/>
                        <table class="pure-table">
                        <thead>
                            <tr>
                                <td>Type</td>
                                <td>#</td>
                                <td><input type="checkbox" onclick="$('.EVENTS_OVERLAY').prop('checked', $('#Checkbox_Stage2Events_OVERLAY').is(':checked'));" checked="checked" id="Checkbox_Stage2Events_OVERLAY"/></td>
                                <td>Size</td>
                                <td>Color</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr id="EVENTS_TR_ShowPriceUpdates_OVERLAY" class="pure-table-odd">
                                <td>
                                    <a class="expander" href="#">Price Updates</a>
                                    <div class="content" style="width: 200px; overflow: auto">
                                        <%= SearchHTML_PriceUpdates_OVERLAY %>
                                    </div>
                                </td>
                                <td><%= this.NumPriceUpdates_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceUpdates_OVERLAY" class="EVENTS_OVERLAY" checked="checked" /></td>
                                <td><input id="Range_PriceUpdates_OVERLAY" type="range" min="1" max="100" step="5" value="1" style="width: 60px"/></td>
                                <td><input id= "Color_PriceUpdates_OVERLAY" type="color" value="#00000" class="CurvedControl50" style="width: 30px" /></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdates_OVERLAY" >
                                <td>
                                    <a class="expander" href="#">News Updates</a>
                                    <div class="content" style="width: 200px; overflow: auto">
                                        <%= SearchHTML_NewsUpdates_OVERLAY %>
                                    </div>
                                </td>
                                <td><%= this.NumNewsUpdates_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdates_OVERLAY" class="EVENTS_OVERLAY" checked="checked" /></td>
                                <td><input id="Range_NewsUpdates_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NewsUpdates_OVERLAY" type="color" value="#ffff00" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNegativeSentiment_OVERLAY" class="pure-table-odd">
                                <td>
                                    <a class="expander" href="#">Sentiment [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NegativeSentiment_OVERLAY%>
                                    </div>
                                </td>
                                <td><%= this.NumNegativeSentiment_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNegativeSentiment_OVERLAY" checked="true" class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_NegativeSentiment_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NegativeSentiment_OVERLAY" type="color" value="#444444" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPositiveSentiment_OVERLAY">
                                <td>
                                    <a class="expander" href="#">Sentiment [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_PositiveSentiment_OVERLAY%>
                                    </div>
                                </td>
                                <td><%= this.NumPositiveSentiment_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPositiveSentiment_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_PositiveSentiment_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PositiveSentiment_OVERLAY" type="color" value="#bbbbbb" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPriceFalls_OVERLAY">
                                <td>                                            
                                    <a class="expander" href="#">Price Falls</a>
                                    <div class="content">
                                        <%= SearchHTML_PriceFalls_OVERLAY%>
                                    </div>
                                </td>
                                <td><%= this.NumPriceFalls_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceFalls_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_PriceFalls_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PriceFalls_OVERLAY" type="color" value="#ff0000" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowPriceJumps_OVERLAY">
                                <td>                                            
                                    <a class="expander" href="#">Price Jumps</a>
                                    <div class="content">
                                        <%= SearchHTML_PriceJumps_OVERLAY%>
                                    </div>
                                </td>
                                <td><%= this.NumPriceJumps_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowPriceJumps_OVERLAY" class="EVENTS_OVERLAY" checked="checked" /></td>
                                <td><input id="Range_PriceJumps_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_PriceJumps_OVERLAY" type="color" value="#00ff00" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdateMinus_OVERLAY">
                                <td>
                                    <a class="expander" href="#">News Update [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsUpdateMinus_OVERLAY%>
                                    </div>
                                </td>
                                <td><%=  this.NumNewsUpdateMinus_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdateMinus_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_NewsUpdateMinus_OVERLAY" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsUpdateMinus" type="color" value="#FF8000" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsUpdatePlus_OVERLAY">
                                <td>
                                    <a class="expander" href="#">News Update [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsUpdatePlus_OVERLAY %>
                                    </div>
                                </td>
                                <td><%= this.NumNewsUpdatePlus_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsUpdatePlus_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_NewsUpdatePlus_OVERLAY" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsUpdatePlus" type="color" value="#87FF00" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStories_OVERLAY">
                                <td>                                            
                                    <a class="expander" href="#">News Stories</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStories_OVERLAY%>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStories_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStories_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_NewsStories_OVERLAY" type="range" min="1" max="100" step="5" value="20" style="width: 60px"/></td>
                                <td><input id= "Color_NewsStories_OVERLAY" type="color" value="#878740" class="CurvedControl50" style="width: 30px"/></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStoryMinus_OVERLAY">
                                <td>
                                    <a class="expander" href="#">News Story [-]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStoryMinus_OVERLAY %>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStoryMinus_OVERLAY %> </td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStoryMinus_OVERLAY" checked="true" Class="EVENTS_OVERLAY" /></td>
                                <td><input id="Range_NewsStoryMinus_OVERLAY" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsStoryMinus" type="color" value="#DF6020" class="CurvedControl50">--%></td>
                            </tr>
                            <tr id="EVENTS_TR_ShowNewsStoryPlus_OVERLAY">
                                <td>
                                    <a class="expander" href="#">News Story [+]</a>
                                    <div class="content">
                                        <%= SearchHTML_NewsStoryPlus_OVERLAY %>
                                    </div>
                                </td>
                                <td><%= this.NumNewsStoryPlus_OVERLAY %></td>
                                <td><input type="checkbox" ID="CheckBox_ShowNewsStoryPlus_OVERLAY" checked="true" Class="EVENTS_OVERLAY"/></td>
                                <td><input id="Range_NewsStoryPlus_OVERLAY" type="range" min="1" max="100" step="5" value="50" style="width: 60px"/></td>
                                <td><%--<input id= "Color_NewsStoryPlus" type="color" value="#60DF20" class="CurvedControl50">--%></td>
                            </tr>
                        </tbody>
                    </table>
                    </div>
                    <br/>
                    <br/>
                    <asp:Button type="button" Text="Show AIO" id="ShowAIO1" runat="server" style="color: Red"  width="150px" height="25px" OnClick="Button_ShowOverlay_Click" />
                </div>
            </div>
        </div>

    </form>
</body>

</html>
