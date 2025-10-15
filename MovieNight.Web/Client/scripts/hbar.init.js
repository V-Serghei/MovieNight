// Client/scripts/hbar.init.js
(function () {
    'use strict';

    function onReady(fn) {
        if (document.readyState !== 'loading') fn();
        else document.addEventListener('DOMContentLoaded', fn, { once: true });
    }

    onReady(function () {
        if (!window.jQuery) { console.error('jQuery is not loaded'); return; }

        (function ($) {
            // ===== селекторы, ожидаемые в _Hbar.cshtml =====
            var $form        = $('#searchForm');              // Ajax.BeginForm (data-ajax="true" ...)
            var $input       = $('#movieSearchInput');        // инпут поиска
            var $results     = $('#searchResults');           // контейнер результатов (dropdown)
            var $closeBtn    = $('#closeBtn');                // кнопка закрытия
            var $closeBox    = $('#closeBtnContainer');       // обертка кнопки
            var $notifCount  = $('#notificationCount');       // бейдж уведомлений
            var noImageUrl   = (window.__urls && window.__urls.noImage) || '/Client/images/Movie/no_image.jpg';
            var unreadUrl    = (window.__urls && window.__urls.unreadCount) || '';
            var lastQueryVal = '';
            var ESC_CODE     = 27;

            // в случае повторного подключения файла не плодим таймеры
            if (window.__hbarUnreadTimer) { clearInterval(window.__hbarUnreadTimer); window.__hbarUnreadTimer = null; }

            function escapeHtml(s) {
                return String(s == null ? '' : s)
                    .replace(/&/g, '&amp;')
                    .replace(/</g, '&lt;')
                    .replace(/>/g, '&gt;')
                    .replace(/"/g, '&quot;')
                    .replace(/'/g, '&#39;');
            }

            function debounce(fn, wait) {
                var t; return function () {
                    var ctx = this, args = arguments;
                    clearTimeout(t); t = setTimeout(function () { fn.apply(ctx, args); }, wait);
                };
            }

            function showResults() {
                if ($results.length) $results.show();
                if ($closeBox.length) $closeBox.show();
            }
            function hideResults() {
                if ($results.length) $results.hide().empty();
                if ($closeBox.length) $closeBox.hide();
            }

            var submitSearch = debounce(function () {
                if (!$form.length) return;
                var v = ($input.val() || '').trim();
                if (v.length < 2 || v === lastQueryVal) return;
                lastQueryVal = v;
                $form.trigger('submit'); // unobtrusive-ajax перехватит
            }, 300);

            // === события ===
            if ($input.length) {
                $input.on('input', function () {
                    var v = (this.value || '').trim();
                    if (v.length > 1) submitSearch();
                    else { lastQueryVal = ''; hideResults(); }
                });

                // ESC — закрыть дропдаун
                $input.on('keydown', function (e) {
                    if (e.which === ESC_CODE) { hideResults(); }
                });
            }

            if ($closeBtn.length) {
                $closeBtn.on('click', function () { hideResults(); });
            }

            // клик вне блока результатов — скрыть
            $(document).on('click.hbar', function (e) {
                if (!$results.length || !$results.is(':visible')) return;
                var $t = $(e.target);
                if (!$t.closest('#searchResults').length && !$t.closest('#movieSearchInput').length) {
                    hideResults();
                }
            });

            // ====== Ajax.BeginForm callbacks (глобальные) ======
            // Укажи их в форме: new AjaxOptions { OnSuccess="handleSuccess", OnFailure="handleFailure" }
            window.handleSuccess = function (data) {
                if (!data || data.success === false || !data.newListV || !Array.isArray(data.newListV) || !data.newListV.length) {
                    hideResults(); return;
                }

                var headerHtml =
                    '<div class="d-flex justify-content-between align-items-center p-2 border-bottom">' +
                    '<strong>Результаты поиска</strong>' +
                    '<button type="button" class="btn btn-sm btn-light" id="searchCloseInline" aria-label="Close">&times;</button>' +
                    '</div>';

                var listHtml = data.newListV.map(function (item) {
                    var poster = (item.PosterImage && item.PosterImage.length) ? item.PosterImage : noImageUrl;
                    var url    = '/InformationSynchronization/MovieTemplatePage/' + encodeURIComponent(item.Id);
                    var title  = escapeHtml(item.Title || '');
                    var year   = escapeHtml(item.ProductionYearS || '');

                    return '' +
                        '<a href="' + url + '" class="dropdown-item notify-item py-2">' +
                        '<div class="notify-icon bg-soft-primary text-primary me-2">' +
                        '<img src="' + poster + '" class="img-fluid rounded-circle" alt="' + title + '" loading="lazy" />' +
                        '</div>' +
                        '<p class="notify-details mb-0">' + title +
                        (year ? ' <small class="text-muted">(' + year + ')</small>' : '') +
                        '</p>' +
                        '</a>';
                }).join('');

                var html = headerHtml + listHtml;
                if ($results.length) {
                    $results.html(html);
                    showResults();

                    // привязываем закрытие к инлайн-кнопке в заголовке
                    $('#searchCloseInline').on('click', hideResults);
                }
            };

            window.handleFailure = function () {
                console.warn('Search AJAX failed');
                hideResults();
            };

            // ===== Счётчик уведомлений =====
            function updateUnreadMessagesCount() {
                if (!unreadUrl) return;
                $.ajax({ url: unreadUrl, type: 'GET' })
                    .done(function (data) {
                        if (!$notifCount.length) return;
                        var count = (data && (data.count || data.Count || data.total || data.Total)) || 0;
                        // показываем пустую строку вместо 0, чтобы бейдж не маячил
                        $notifCount.text(count > 0 ? String(count) : '');
                    })
                    .fail(function () {
                        // не шумим в консоль каждую минуту
                    });
            }
            updateUnreadMessagesCount();
            window.__hbarUnreadTimer = setInterval(updateUnreadMessagesCount, 60000);

        })(jQuery);
    });
})();
