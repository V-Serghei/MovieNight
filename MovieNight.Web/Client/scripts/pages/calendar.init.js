/*
Template Name: Minton - Responsive Bootstrap 4 Admin Dashboard
Author: CoderThemes
Website: https://coderthemes.com/
Contact: support@coderthemes.com
File: Calendar
*/

!function($) {
    "use strict";

    var CalendarApp = function() {
        this.$body = $("body")
        this.$modal = $('#event-modal'),
            this.$event = ('#external-events div.external-event'),
            this.$calendar = $('#calendar'),
            this.$saveCategoryBtn = $('.save-category'),
            this.$categoryForm = $('#add-category form'),
            this.$extEvents = $('#external-events'),
            this.$calendarObj = null
    };

    /* on drop */
    CalendarApp.prototype.onDrop = function (eventObj, date) {
        var $this = this;
        var originalEventObject = eventObj.data('eventObject');
        var $categoryClass = eventObj.attr('data-class');
        var copiedEventObject = $.extend({}, originalEventObject);
        copiedEventObject.start = date;
        if ($categoryClass)
            copiedEventObject['className'] = [$categoryClass];
        $this.$calendar.fullCalendar('renderEvent', copiedEventObject, true);
        if ($('#drop-remove').is(':checked')) {
            eventObj.remove();
        }
    };

    /* on click on event */
    CalendarApp.prototype.onEventClick = function (calEvent, jsEvent, view) {
        var $this = this;
        var form = $("<form></form>");
        form.append("<label>Change event name</label>");
        form.append("<div class='input-group m-b-15'><input class='form-control' type=text value='" + calEvent.title + "' /><span class='input-group-append'><button type='submit' class='btn btn-success btn-md'><i class='fa fa-check'></i> Save</button></span></div>");
        $this.$modal.modal({
            backdrop: 'static'
        });
        $this.$modal.find('.delete-event').show().end().find('.save-event').hide().end().find('.modal-body').empty().prepend(form).end().find('.delete-event').unbind('click').click(function () {
            $this.$calendarObj.fullCalendar('removeEvents', function (ev) {
                return (ev._id == calEvent._id);
            });
            $this.$modal.modal('hide');
        });
        $this.$modal.find('form').on('submit', function () {
            calEvent.title = form.find("input[type=text]").val();
            $this.$calendarObj.fullCalendar('updateEvent', calEvent);
            $this.$modal.modal('hide');
            return false;
        });
    };

    /* on select */
    CalendarApp.prototype.onSelect = function (start, end, allDay) {
        var $this = this;
        $this.$modal.modal({
            backdrop: 'static'
        });
        var form = $("<form></form>");
        form.append("<div class='row'></div>");
        form.find(".row")
            .append("<div class='col-12'><div class='form-group'><label class='control-label'>Event Name</label><input class='form-control' placeholder='Insert Event Name' type='text' name='title'/></div></div>")
            .append("<div class='col-12'><div class='form-group'><label class='control-label'>Category</label><select class='form-control' name='category'></select></div></div>")
            .find("select[name='category']")
            .append("<option value='bg-danger'>Danger</option>")
            .append("<option value='bg-success'>Success</option>")
            .append("<option value='bg-primary'>Primary</option>")
            .append("<option value='bg-info'>Info</option>")
            .append("<option value='bg-dark'>Dark</option>")
            .append("<option value='bg-warning'>Warning</option></div></div>");
        $this.$modal.find('.delete-event').hide().end().find('.save-event').show().end().find('.modal-body').empty().prepend(form).end().find('.save-event').unbind('click').click(function () {
            form.submit();
        });
        $this.$modal.find('form').on('submit', function () {
            var title = form.find("input[name='title']").val();
            var categoryClass = form.find("select[name='category'] option:checked").val();
            if (title !== null && title.length != 0) {
                var eventData = {
                    title: title,
                    start: start,
                    end: end,
                    category: categoryClass
                };

                // AJAX request to save the event
                $.ajax({
                    url: '/Notification/EventSave', // Update with your actual controller and method
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(eventData),
                    success: function(response) {
                        if (response.success) {
                            $this.$calendarObj.fullCalendar('renderEvent', {
                                title: title,
                                start: start,
                                end: end,
                                allDay: false,
                                className: categoryClass
                            }, true);
                            alert('Event saved successfully!');
                        } else {
                            alert('Failed to save event: ' + response.message);
                        }
                        $this.$modal.modal('hide');
                    },
                    error: function() {
                        alert('An error occurred while saving the event.');
                        $this.$modal.modal('hide');
                    }
                });
            } else {
                alert('You have to give a title to your event');
            }
            return false;
        });
        $this.$calendarObj.fullCalendar('unselect');
    };

    CalendarApp.prototype.enableDrag = function() {
        $(this.$event).each(function () {
            var eventObject = {
                title: $.trim($(this).text())
            };
            $(this).data('eventObject', eventObject);
            $(this).draggable({
                zIndex: 999,
                revert: true,
                revertDuration: 0
            });
        });
    };

    /* Initializing */
    CalendarApp.prototype.init = function() {
        this.enableDrag();
        var date = new Date();
        var d = date.getDate();
        var m = date.getMonth();
        var y = date.getFullYear();
        var form = '';
        var today = new Date($.now());

        var defaultEvents =  [{
            title: 'Hey!',
            start: new Date($.now() + 158000000),
            className: 'bg-warning'
        },
            {
                title: 'See John Deo',
                start: today,
                end: today,
                className: 'bg-success'
            },
            {
                title: 'Meet John Deo',
                start: new Date($.now() + 168000000),
                className: 'bg-info'
            },
            {
                title: 'Buy a Theme',
                start: new Date($.now() + 338000000),
                className: 'bg-primary'
            }];

        var $this = this;
        $this.$calendarObj = $this.$calendar.fullCalendar({
            slotDuration: '00:15:00',
            minTime: '08:00:00',
            maxTime: '19:00:00',
            defaultView: 'month',
            handleWindowResize: true,
            height: $(window).height() - 200,
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            events: defaultEvents,
            editable: true,
            droppable: true,
            eventLimit: true,
            selectable: true,
            drop: function(date) { $this.onDrop($(this), date); },
            select: function (start, end, allDay) { $this.onSelect(start, end, allDay); },
            eventClick: function(calEvent, jsEvent, view) { $this.onEventClick(calEvent, jsEvent, view); }
        });

        //on new event
        this.$saveCategoryBtn.on('click', function(){
            var categoryName = $this.$categoryForm.find("input[name='category-name']").val();
            var categoryColor = $this.$categoryForm.find("select[name='category-color']").val();
            if (categoryName !== null && categoryName.length != 0) {
                $this.$extEvents.append('<div class="external-event bg-' + categoryColor + '" data-class="bg-' + categoryColor + '" style="position: relative;"><i class="mdi mdi-checkbox-blank-circle mr-2 vertical-middle"></i>' + categoryName + '</div>')
                $this.enableDrag();
            }
        });
    };

    //init CalendarApp
    $.CalendarApp = new CalendarApp, $.CalendarApp.Constructor = CalendarApp

}(window.jQuery),

//initializing CalendarApp
    function($) {
        "use strict";
        $.CalendarApp.init()
    }(window.jQuery);
