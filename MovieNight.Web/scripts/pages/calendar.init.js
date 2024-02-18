!(function (o) {
    "use strict";
    var e = function () {
        (this.$body = o("body")),
            (this.$modal = o("#event-modal")),
            (this.$event = "#external-events div.external-event"),
            (this.$calendar = o("#calendar")),
            (this.$saveCategoryBtn = o(".save-category")),
            (this.$categoryForm = o("#add-category form")),
            (this.$extEvents = o("#external-events")),
            (this.$calendarObj = null);
    };
    (e.prototype.onDrop = function (e, t) {
        var n = e.data("eventObject"),
            a = e.attr("data-class"),
            l = o.extend({}, n);
        (l.start = t),
            a && (l.className = [a]),
            this.$calendar.fullCalendar("renderEvent", l, !0),
            o("#drop-remove").is(":checked") && e.remove();
    }),
        (e.prototype.onEventClick = function (t, e, n) {
            var a = this,
                l = o("<form></form>");
            l.append("<label>Change event name</label>"),
                l.append(
                    "<div class='input-group m-b-15'><input class='form-control' type=text value='" +
                    t.title +
                    "' /><span class='input-group-append'><button type='submit' class='btn btn-success btn-md  '><i class='fa fa-check'></i> Save</button></span></div>",
                ),
                a.$modal.modal({ backdrop: "static" }),
                a.$modal
                    .find(".delete-event")
                    .show()
                    .end()
                    .find(".save-event")
                    .hide()
                    .end()
                    .find(".modal-body")
                    .empty()
                    .prepend(l)
                    .end()
                    .find(".delete-event")
                    .unbind("click")
                    .click(function () {
                        a.$calendarObj.fullCalendar("removeEvents", function (e) {
                            return e._id == t._id;
                        }),
                            a.$modal.modal("hide");
                    }),
                a.$modal.find("form").on("submit", function () {
                    return (
                        (t.title = l.find("input[type=text]").val()),
                        a.$calendarObj.fullCalendar("updateEvent", t),
                        a.$modal.modal("hide"),
                        !1
                    );
                });
        }),
        (e.prototype.onSelect = function (n, a, e) {
            var l = this;
            l.$modal.modal({ backdrop: "static" });
            var i = o("<form></form>");
            i.append("<div class='row'></div>"),
                i
                    .find(".row")
                    .append(
                        "<div class='col-12'><div class='form-group'><label class='control-label'>Event Name</label><input class='form-control' placeholder='Insert Event Name' type='text' name='title'/></div></div>",
                    )
                    .append(
                        "<div class='col-12'><div class='form-group'><label class='control-label'>Category</label><select class='form-control' name='category'></select></div></div>",
                    )
                    .find("select[name='category']")
                    
                .append("<option value='bg-success'>I need to see it</option>")
                    .append("<option value='bg-primary'>I want to see it</option>")
                .append("<option value='bg-info'>Maybe</option>")
                .append("<option value='bg-dark'>I really want to see it</option>")
                    .append(
                        "<option value='bg-warning'>I don't want to see it</option></div></div>",
                    ),
                l.$modal
                    .find(".delete-event")
                    .hide()
                    .end()
                    .find(".save-event")
                    .show()
                    .end()
                    .find(".modal-body")
                    .empty()
                    .prepend(i)
                    .end()
                    .find(".save-event")
                    .unbind("click")
                    .click(function () {
                        i.submit();
                    }),
                l.$modal.find("form").on("submit", function () {
                    var e = i.find("input[name='title']").val(),
                        t =
                            (i.find("input[name='beginning']").val(),
                                i.find("input[name='ending']").val(),
                                i.find("select[name='category'] option:checked").val());
                    return (
                        null !== e && 0 != e.length
                            ? (l.$calendarObj.fullCalendar(
                                "renderEvent",
                                { title: e, start: n, end: a, allDay: !1, className: t },
                                !0,
                            ),
                                l.$modal.modal("hide"))
                            : alert("You have to give a title to your event"),
                        !1
                    );
                }),
                l.$calendarObj.fullCalendar("unselect");
        }),
        (e.prototype.enableDrag = function () {
            o(this.$event).each(function () {
                var e = { title: o.trim(o(this).text()) };
                o(this).data("eventObject", e),
                    o(this).draggable({ zIndex: 999, revert: !0, revertDuration: 0 });
            });
        }),
        (e.prototype.init = function () {
            this.enableDrag();
            var e = new Date(),
                t = (e.getDate(), e.getMonth(), e.getFullYear(), new Date(o.now())),
                n = [
                    {
                        title: "Hey!",
                        start: new Date(o.now() + 158e6),
                        className: "bg-warning",
                    },
                    { title: "See John Deo", start: t, end: t, className: "bg-success" },
                    {
                        title: "Meet John Deo",
                        start: new Date(o.now() + 168e6),
                        className: "bg-info",
                    },
                    {
                        title: "Buy a Theme",
                        start: new Date(o.now() + 338e6),
                        className: "bg-primary",
                    },
                ],
                a = this;
            (a.$calendarObj = a.$calendar.fullCalendar({
                slotDuration: "00:15:00",
                minTime: "08:00:00",
                maxTime: "19:00:00",
                defaultView: "month",
                handleWindowResize: !0,
                height: o(window).height() - 200,
                header: {
                    left: "prev,next today",
                    center: "title",
                    right: "month,agendaWeek,agendaDay",
                },
                events: n,
                editable: !0,
                droppable: !0,
                eventLimit: !0,
                selectable: !0,
                drop: function (e) {
                    a.onDrop(o(this), e);
                },
                select: function (e, t, n) {
                    a.onSelect(e, t, n);
                },
                eventClick: function (e, t, n) {
                    a.onEventClick(e, t, n);
                },
            })),
                this.$saveCategoryBtn.on("click", function () {
                    var e = a.$categoryForm.find("input[name='category-name']").val(),
                        t = a.$categoryForm.find("select[name='category-color']").val();
                    null !== e &&
                        0 != e.length &&
                        (a.$extEvents.append(
                            '<div class="external-event bg-' +
                            t +
                            '" data-class="bg-' +
                            t +
                            '" style="position: relative;"><i class="mdi mdi-checkbox-blank-circle mr-2 vertical-middle"></i>' +
                            e +
                            "</div>",
                        ),
                            a.enableDrag());
                });
        }),
        (o.CalendarApp = new e()),
        (o.CalendarApp.Constructor = e);
})(window.jQuery),
    (function (e) {
        "use strict";
        window.jQuery.CalendarApp.init();
    })();
